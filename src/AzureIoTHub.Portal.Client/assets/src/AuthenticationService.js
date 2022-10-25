"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AuthenticationService = exports.AuthenticationResultStatus = exports.AccessTokenResultStatus = void 0;
const oidc_client_1 = require("oidc-client");
oidc_client_1.Log.level = oidc_client_1.Log.DEBUG;
oidc_client_1.Log.logger = console;
function isApiAuthorizationSettings(settings) {
    return settings.hasOwnProperty('configurationEndpoint');
}
var AccessTokenResultStatus;
(function (AccessTokenResultStatus) {
    AccessTokenResultStatus["Success"] = "success";
    AccessTokenResultStatus["RequiresRedirect"] = "requiresRedirect";
})(AccessTokenResultStatus = exports.AccessTokenResultStatus || (exports.AccessTokenResultStatus = {}));
var AuthenticationResultStatus;
(function (AuthenticationResultStatus) {
    AuthenticationResultStatus["Redirect"] = "redirect";
    AuthenticationResultStatus["Success"] = "success";
    AuthenticationResultStatus["Failure"] = "failure";
    AuthenticationResultStatus["OperationCompleted"] = "operationCompleted";
})(AuthenticationResultStatus = exports.AuthenticationResultStatus || (exports.AuthenticationResultStatus = {}));
;
class OidcAuthorizeService {
    constructor(userManager) {
        this._userManager = userManager;
    }
    async trySilentSignIn() {
        if (!this._intialSilentSignIn) {
            this._intialSilentSignIn = (async () => {
                try {
                    await this._userManager.signinSilent();
                }
                catch (e) {
                    // It is ok to swallow the exception here.
                    // The user might not be logged in and in that case it
                    // is expected for signinSilent to fail and throw
                }
            })();
        }
        return this._intialSilentSignIn;
    }
    async getUser() {
        if (window.parent === window && !window.opener && !window.frameElement && this._userManager.settings.redirect_uri &&
            !location.href.startsWith(this._userManager.settings.redirect_uri)) {
            // If we are not inside a hidden iframe, try authenticating silently.
            await AuthenticationService.instance.trySilentSignIn();
        }
        const user = await this._userManager.getUser();
        return user && user.profile;
    }
    async getAccessToken(request) {
        const user = await this._userManager.getUser();
        if (hasValidAccessToken(user) && hasAllScopes(request, user.scopes)) {
            return {
                status: AccessTokenResultStatus.Success,
                token: {
                    grantedScopes: user.scopes,
                    expires: getExpiration(user.expires_in),
                    value: user.access_token
                }
            };
        }
        else {
            try {
                const parameters = request && request.scopes ?
                    { scope: request.scopes.join(' ') } : undefined;
                const newUser = await this._userManager.signinSilent(parameters);
                return {
                    status: AccessTokenResultStatus.Success,
                    token: {
                        grantedScopes: newUser.scopes,
                        expires: getExpiration(newUser.expires_in),
                        value: newUser.access_token
                    }
                };
            }
            catch (e) {
                return {
                    status: AccessTokenResultStatus.RequiresRedirect
                };
            }
        }
        function hasValidAccessToken(user) {
            return !!(user && user.access_token && !user.expired && user.scopes);
        }
        function getExpiration(expiresIn) {
            const now = new Date();
            now.setTime(now.getTime() + expiresIn * 1000);
            return now;
        }
        function hasAllScopes(request, currentScopes) {
            const set = new Set(currentScopes);
            if (request && request.scopes) {
                for (const current of request.scopes) {
                    if (!set.has(current)) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    async signIn(state) {
        try {
            await this._userManager.clearStaleState();
            await this._userManager.signinSilent(this.createArguments());
            return this.success(state);
        }
        catch (silentError) {
            try {
                await this._userManager.clearStaleState();
                await this._userManager.signinRedirect(this.createArguments(state));
                return this.redirect();
            }
            catch (redirectError) {
                return this.error(this.getExceptionMessage(redirectError));
            }
        }
    }
    async completeSignIn(url) {
        const requiresLogin = await this.loginRequired(url);
        const stateExists = await this.stateExists(url);
        try {
            const user = await this._userManager.signinCallback(url);
            if (window.self !== window.top) {
                return this.operationCompleted();
            }
            else {
                return this.success(user && user.state);
            }
        }
        catch (error) {
            if (requiresLogin || window.self !== window.top || !stateExists) {
                return this.operationCompleted();
            }
            return this.error('There was an error signing in.');
        }
    }
    async signOut(state) {
        try {
            if (!(await this._userManager.metadataService.getEndSessionEndpoint())) {
                await this._userManager.removeUser();
                return this.success(state);
            }
            await this._userManager.signoutRedirect(this.createArguments(state));
            return this.redirect();
        }
        catch (redirectSignOutError) {
            return this.error(this.getExceptionMessage(redirectSignOutError));
        }
    }
    async completeSignOut(url) {
        try {
            if (await this.stateExists(url)) {
                const response = await this._userManager.signoutCallback(url);
                return this.success(response && response.state);
            }
            else {
                return this.operationCompleted();
            }
        }
        catch (error) {
            return this.error(this.getExceptionMessage(error));
        }
    }
    getExceptionMessage(error) {
        if (isOidcError(error)) {
            return error.error_description;
        }
        else if (isRegularError(error)) {
            return error.message;
        }
        else {
            return error.toString();
        }
        function isOidcError(error) {
            return error && error.error_description;
        }
        function isRegularError(error) {
            return error && error.message;
        }
    }
    async stateExists(url) {
        const stateParam = new URLSearchParams(new URL(url).search).get('state');
        if (stateParam && this._userManager.settings.stateStore) {
            return await this._userManager.settings.stateStore.get(stateParam);
        }
        else {
            return undefined;
        }
    }
    async loginRequired(url) {
        const errorParameter = new URLSearchParams(new URL(url).search).get('error');
        if (errorParameter && this._userManager.settings.stateStore) {
            const error = await this._userManager.settings.stateStore.get(errorParameter);
            return error === 'login_required';
        }
        else {
            return false;
        }
    }
    createArguments(state) {
        return { useReplaceToNavigate: true, data: state };
    }
    error(message) {
        return { status: AuthenticationResultStatus.Failure, errorMessage: message };
    }
    success(state) {
        return { status: AuthenticationResultStatus.Success, state };
    }
    redirect() {
        return { status: AuthenticationResultStatus.Redirect };
    }
    operationCompleted() {
        return { status: AuthenticationResultStatus.OperationCompleted };
    }
}
class AuthenticationService {
    static init(settings) {
        // Multiple initializations can start concurrently and we want to avoid that.
        // In order to do so, we create an initialization promise and the first call to init
        // tries to initialize the app and sets up a promise other calls can await on.
        if (!AuthenticationService._initialized) {
            AuthenticationService._initialized = AuthenticationService.initializeCore(Object.assign({}, {
                loadUserInfo: false
            }, settings));
        }
        return AuthenticationService._initialized;
    }
    static handleCallback() {
        return AuthenticationService.initializeCore();
    }
    static async initializeCore(settings) {
        const finalSettings = settings || AuthenticationService.resolveCachedSettings();
        if (!settings && finalSettings) {
            const userManager = AuthenticationService.createUserManagerCore(finalSettings);
            if (window.parent !== window && !window.opener && (window.frameElement && userManager.settings.redirect_uri &&
                location.href.startsWith(userManager.settings.redirect_uri))) {
                // If we are inside a hidden iframe, try completing the sign in early.
                // This prevents loading the blazor app inside a hidden iframe, which speeds up the authentication operations
                // and avoids wasting resources (CPU and memory from bootstrapping the Blazor app)
                AuthenticationService.instance = new OidcAuthorizeService(userManager);
                // This makes sure that if the blazor app has time to load inside the hidden iframe,
                // it is not able to perform another auth operation until this operation has completed.
                AuthenticationService._initialized = (async () => {
                    await AuthenticationService.instance.completeSignIn(location.href);
                    return;
                })();
            }
        }
        else if (settings) {
            const userManager = await AuthenticationService.createUserManager(settings);
            AuthenticationService.instance = new OidcAuthorizeService(userManager);
        }
        else {
            // HandleCallback gets called unconditionally, so we do nothing for normal paths.
            // Cached settings are only used on handling the redirect_uri path and if the settings are not there
            // the app will fallback to the default logic for handling the redirect.
        }
    }
    static resolveCachedSettings() {
        const cachedSettings = window.sessionStorage.getItem(`${AuthenticationService._infrastructureKey}.CachedAuthSettings`);
        return cachedSettings ? JSON.parse(cachedSettings) : undefined;
    }
    static getUser() {
        return AuthenticationService.instance.getUser();
    }
    static getAccessToken(options) {
        return AuthenticationService.instance.getAccessToken(options);
    }
    static signIn(state) {
        return AuthenticationService.instance.signIn(state);
    }
    static async completeSignIn(url) {
        let operation = this._pendingOperations[url];
        if (!operation) {
            operation = AuthenticationService.instance.completeSignIn(url);
            await operation;
            delete this._pendingOperations[url];
        }
        return operation;
    }
    static signOut(state) {
        return AuthenticationService.instance.signOut(state);
    }
    static async completeSignOut(url) {
        let operation = this._pendingOperations[url];
        if (!operation) {
            operation = AuthenticationService.instance.completeSignOut(url);
            await operation;
            delete this._pendingOperations[url];
        }
        return operation;
    }
    static async createUserManager(settings) {
        let finalSettings;
        if (isApiAuthorizationSettings(settings)) {
            const response = await fetch(settings.configurationEndpoint);
            if (!response.ok) {
                throw new Error(`Could not load settings from '${settings.configurationEndpoint}'`);
            }
            const downloadedSettings = await response.json();
            finalSettings = downloadedSettings;
        }
        else {
            if (!settings.scope) {
                settings.scope = settings.defaultScopes.join(' ');
            }
            if (settings.response_type === null) {
                // If the response type is not set, it gets serialized as null. OIDC-client behaves differently than when the value is undefined, so we explicitly check for a null value and remove the property instead.
                delete settings.response_type;
            }
            finalSettings = settings;
        }
        window.sessionStorage.setItem(`${AuthenticationService._infrastructureKey}.CachedAuthSettings`, JSON.stringify(finalSettings));
        return AuthenticationService.createUserManagerCore(finalSettings);
    }
    static createUserManagerCore(finalSettings) {
        const userManager = new oidc_client_1.UserManager(finalSettings);
        userManager.events.addUserSignedOut(async () => {
            userManager.removeUser();
        });
        return userManager;
    }
}
exports.AuthenticationService = AuthenticationService;
AuthenticationService._infrastructureKey = 'Microsoft.AspNetCore.Components.WebAssembly.Authentication';
AuthenticationService._pendingOperations = {};
AuthenticationService.handleCallback();
window.AuthenticationService = AuthenticationService;
//# sourceMappingURL=AuthenticationService.js.map
// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using Microsoft.EntityFrameworkCore.Migrations;
    using Microsoft.EntityFrameworkCore.Migrations.Operations;

    [TestFixture]
    public class LayerAssignmentMigrationTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void Up_AssignmentMigration_MakesPlanningNullableAndConvertsLegacyValues(bool mysql)
        {
            Migration migration = mysql
                ? new IoTHub.Portal.MySql.Migrations.UseNullableLayerAssignments()
                : new IoTHub.Portal.Postgres.Migrations.UseNullableLayerAssignments();

            var alter = migration.UpOperations.OfType<AlterColumnOperation>().Single();
            Assert.AreEqual("Layers", alter.Table);
            Assert.AreEqual("Planning", alter.Name);
            Assert.IsTrue(alter.IsNullable);

            var sql = migration.UpOperations.OfType<SqlOperation>().Select(operation => operation.Sql).ToList();
            Assert.AreEqual(2, sql.Count);
            StringAssert.Contains("Layers", sql[0]);
            StringAssert.Contains("Planning", sql[0]);
            StringAssert.Contains("NULL", sql[0]);
            StringAssert.Contains("'None'", sql[0]);
            StringAssert.Contains("Devices", sql[1]);
            StringAssert.Contains("LayerId", sql[1]);
            StringAssert.Contains("NULL", sql[1]);
            StringAssert.Contains("'None'", sql[1]);
        }
    }
}

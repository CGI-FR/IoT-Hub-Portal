---
title: "Discussion: {{ payload.discussion.title }}"
labels: discussion, refinement

---

## Discussed in [{{ payload.discussion.title }}]({{ payload.discussion.html_url }})
__Originally posted by **[{{ payload.discussion.user.login }}]({{ payload.discussion.user.url }})**__ {{ payload.discussion.created_at | date("dddd, MMMM Do YYYY, h:mm:ss a") }}

## Idea 

{{ payload.discussion.body }}
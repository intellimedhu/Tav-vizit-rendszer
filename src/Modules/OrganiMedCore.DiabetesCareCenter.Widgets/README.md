# OrganiMedCore.DiabetesCareCenter.Widgets

This module provides a way to be able to add center profile related widgets.

## Liquid Filters

### `CenterProfileListReviewFilter`

```
{% if User | has_centerprofiles_to_review %}
...
{% endif %}
```

### `CenterProfileReviewFilter`

```
{% if User | centerprofile_review_authorized %}
...
{% endif %}
```

### `ColleagueWorkplaceZoneFilter`
```
{% assign zones = User | colleaguestatus_zones %}
...
{% endif %}
```
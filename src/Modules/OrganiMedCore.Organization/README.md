# Organization Core


## Organization properties

It can be retrieved with IOrganizationService.GetOrganizationAsync. 
Id: the tenant's name. 
Name: the site name in default sitesettings. 
EESZT ID and name: stored in sitesetting in the OrganizationSettings.

## Storing metadata about the content items

Organization unit ID: stored in MetaDataPart it will be set automatically in it's driver.
Patient ID: stored in MetaDataPart and must be set manually in a controller or elsewhere.
Organization ID: it will be stored on the organization unit automatically so it can be retrieved with the organization unit ID.
Creator user name: stored automatically in the content item's Owner property.
Modifier user name: stored automatically in the content item's Author property.
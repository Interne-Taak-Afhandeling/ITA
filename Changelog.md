## vx.x.x

### New features

* Email notifications contain only a deeplink to contact request (no confidential information) #368
* Role-based access control for "Beheer" functionality - only users with "Functioneel Beheerder" role can access and manage "Kanalenlijst" #375



### Warnings and deployment notes

* New required release variable: `Ita_BaseUrl` - The base URL of the ITA website for deeplinks in email notifications
* New required release variable: `Oidc_Functioneel_Beheerder_Role` - The Azure AD role name for Functioneel Beheerder access (e.g., "ITA-Functioneel-Beheerder")
* Azure AD configuration required: Create and assign the "ITA-Functioneel-Beheerder" app role to users who need access to management features

### Bugfixes

* helm: make sure the website restarts when config or secret changes

### Maintenance

* Upgrade Vue

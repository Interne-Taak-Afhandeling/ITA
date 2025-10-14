## vx.x.x

### New features

* Create logboek object and handle action logging \& extraction #163
* Handle action logging \& extraction for remaining actions #164
* Add a note to a contact request #14
* View completed contact requests from employee #18
* Overview of contact requests for department or group #19
* View completed contact requests for department or group #54
* Manage the list of "Kanalen" #174
* Forward a contact request #16
* Receive an email for a forwarded contactrequest #108
* Email notifications contain only a deeplink to contact request (no confidential information) #368
* Role-based access control for "Beheer" functionality - only users with "Functioneel Beheerder" role can access and manage "Kanalenlijst" #375



### Warnings and deployment notes

* New required release variable: `Ita_BaseUrl` - The base URL of the ITA website for deeplinks in email notifications
* New required release variable: `OIDC_FUNCTIONEEL_BEHEERDER_ROLE` - The Azure AD role name for Functioneel Beheerder access (e.g., "ITA-Functioneel-Beheerder")
* Azure AD configuration required: Create and assign the "ITA-Functioneel-Beheerder" app role to users who need access to management features

### Bugfixes

* helm: make sure the website restarts when config or secret changes

### Maintenance

* Upgrade Vue

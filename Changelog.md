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



### Warnings and deployment notes

* New required release variable: `Ita_BaseUrl` - The base URL of the ITA website for deeplinks in email notifications

### Bugfixes

* helm: make sure the website restarts when config or secret changes

### Maintenance

* Upgrade Vue

# OrgDocs
Organizational documentation manager w/ ASP.NET core MVC


## Features

* **Subscription** : They key feature of this documentation manager is that end users can subscribe to individual company
documentations so that they are notified anytime a new update is available. Currently the update notification is sent via email and configured
with SendGrid. 

* **Administration**: Admins can manage users, assign them to roles and upload/update documents

* **End Users**: End Users can browse documents by  filtering via categories, departments, keywords and/or sorting. Initial user registration/login is managed
through Identity, and post-registration Administrators can approve and assign users to roles to allow for the subscription feature.





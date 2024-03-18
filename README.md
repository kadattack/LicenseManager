# LicenseManager

Work in progress. The Manager is still not fully functional to the point where it can be used.


Repository contains 3 projects


### LicenseValidationServer
This server accepts REST API requests from the client to check if the license is activated and returns permission on whether the client can continue.
It uses ECDiffieHellman for encryption and ECDsa for signature vertification beaneath the regular TLS to prevent license tinkering and spoofing false packages.

#### TODOs

- [ ] Implement standardized license information
- [ ] Add optional hardware license lock


### LicenseValidation
This library is meant to serve functions for the client application to communicate with LicenseValidationServer. Currently it's works so that it check license validation at the start of the application and then by setting a timer to check every 30 days. 


#### TODOs

- [ ] Research to see if there's a better method to checking license validity




### LicenseManagerMVVM
A destkop application that uses Avalonia framework to help users manage licenses through a UI. It's meant to create new clients, licenses, track payments, etc.
![Screenshot from 2024-03-18 17-51-01](https://github.com/kadattack/LicenseManager/assets/78758877/48355d53-ae42-4f64-886c-6c899a1e7044)

#### TODOs

- [ ] Work in progress and unfinctional at the moment

# Deploying the solution on the Raspberry Pi

Since this is a rather simple solution that only runs on my own Raspberry Pi, this solution is being deployed manually.  I'm not making use of DPS or whatsoever.

## Pushing the modules to a container registry

First of all, the modules that have been developed, need to be published to a Container Registry that is accessible for the Azure IoT Edge runtime that is running on the Raspberry Pi.

I'm using an Azure Container Registry instance to which the modules will be pushed.
To do so, following steps need to be taken:

- Make sure that the `module.json` file refers to the Container Registry to which the module needs to be pushed to.  This is done by specifying the address of the (in my case) ACR and the modulename in the `repository` property of the `module.json` file:
  
   ```json
   ...
   "image": {
        "repository": "<acrname>.azurecr.io/<modulename>"
   }
   ...
   ```

   This should all be in lower-case (ACR name and modulename).

- Make sure that all required information that is necessary to publish the container is present in the `deployment.template.json` file.
  The address of the ACR must be present and the credentials that can be used to push images must be specified as well.
  To prevent that the username/password is present in the source-code repository, it is advised to use variables for this sensitive information:

  ```json
  "registryCredentials": {
              "tempflowcontainers": {
                "username": "$CONTAINER_REGISTRY_USERNAME_tempflowcontainers",
                "password": "$CONTAINER_REGISTRY_PASSWORD_tempflowcontainers",
                "address": "tempflowcontainers.azurecr.io"
              }
            }
  ```

  The `username` and `password` properties refer to variables that are defined in the `.env` file in Visual Studio Code.

- Once all this is done, you should be able to build & publish the module to the Container Registry.  This can be done by right-clicking the `module.json` file and select `Build and Push IoT Edge module image` or via the Command Palette and executing the `Build and Push IoT Edge module image` command.

## Getting the solution on the Pi

- Create an IoT Edge device in IoT Hub
- Specify the modules that must be deployed to that device by clicking `Set Modules`.  (If the solution needs to be deployed to multiple devices, a Deployment should be created instead of creating a single IoT Edge device manually, but this is outside the scope of this little project)
- Specify the modules that must run on the device.  Also, make sure that the Container Registry Settings are correctly specified and point to the Container Registry that holds the docker containers that must be retrieved.

- SSH to the Raspberry Pi
- edit the `config.yaml` file that is found in `/etc/iotedge`
- Paste the connectionstring to the IoT Edge device from IoT Hub into the config.yaml file
- Restart the IoT Edge runtime:
  
  ```
  sudo systemctl restart iotedge
  ```

- verify if everything is running ok by executing
  ```
  sudo systemctl status iotedge
  ```
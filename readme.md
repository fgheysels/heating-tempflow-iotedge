#Heating Tempflow on Azure IoT Edge

## Install IoT Edge Runtime on Raspberry Pi

The steps to install Azure IoT edge on the Rpi are outlined [here](https://thenewstack.io/tutorial-connect-and-configure-raspberry-pi-as-an-azure-iot-edge-device/).

## Prepare dev machine

- install iot edge thing for VS.NET or VS Code
- install iotedgehubdev
  (first install python)
  then 
  pip install --upgrade iotedgehubdev
  
  https://docs.microsoft.com/en-us/azure/iot-edge/how-to-vs-code-develop-module

  https://www.makeuseof.com/tag/install-pip-for-python/

## Make sure the Iot Edge Module works on Raspbian

- Raspbian is 32 bit, so we need a dockerfile for that platform

## Push container to ACR

### via VS.NET
 
  - make sure that the `repository` property in the modules.json contains the address of the ACR and the modulename, like this: `<acr-name>.azurecr.io/<modulename>`
  - The repository name should be in lowercase

  - Right click, build and push IoT edge modules
  - https://docs.microsoft.com/en-us/azure/iot-edge/how-to-visual-studio-develop-csharp-module#build-and-push-images

- you need to make sure that the credentials are present in the deployment.json file, preferable via environment variables

## get the module on the pi

- create an IoT Edge device in IoT Hub
- Make sure to create a deployment; add the module that has been pushed to ACR by selecting 'set modules'
  
- edit config.yaml in /etc/iotedge
- paste the connectionstring to the iot edge device from IoT hub into the config file
- restart iot edge runtime:
  ```
  sudo systemctl restart iotedge
  ```
- verify if everyting is ok by running
  ```
  sudo systemctl status iotedge
  ```
- edgeHub module was not running; fixed by specifying another version in deployment.json:
  specified 
  ```
  "image": "mcr.microsoft.com/azureiotedge-hub:1.0.6"
  ```
  instead of 
  ```
  "image": "mcr.microsoft.com/azureiotedge-hub:1.0"
  ```

- if IoT Edge cannot pull the container for your module from the container registry, you probably did not enter the container-information in the Container Registry Settings in the Azure Portal under 'set modules'.

- display logs / status of IoT module:

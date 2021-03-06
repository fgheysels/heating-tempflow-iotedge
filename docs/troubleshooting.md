# Troubleshooting IoT Edge on Raspbian

## Getting runtime log information

- View the status of IoT Edge:
  
  ```
  sudo systemctl status iotedge
  ```

- Display logs of an IoT module:
  
  ```
  iotedge logs <containername>
  ```

- When a custom-module is not able to start / not running, it might be helpful to check the logs of the iotEdgeAgent or iotEdgeHub module:

  ```
  iotedge logs edgeAgent
  ```

  or

  ```
  iotedge logs edgeHub
  ```

- If the logs are too large, it might be usefull to use the `tail` option:

  ```
  iotedge logs edgeHub --tail 25
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

## Debugging custom module

This [article](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-vs-code-develop-module#debug-a-module-without-a-container-c-nodejs-java) explains how to debug a custom module.  Note that the iotedgehubdev module must be installed.  See the [prepare dev environment](prepare-dev-environment.md) page.
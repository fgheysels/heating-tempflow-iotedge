# Heating Tempflow on Azure IoT Edge

## Introduction

This project is a reworked version of 'Heating tempflow'.  In the original project, I've used a Python script to read out temperature sensors and posted the values to an InfluxDB instance that was running on the Raspberry Pi itself, the values where visualized using Grafana.

In this version of the project, I'm making use of Azure IoT Edge.  A custom module (container) is hosted in Azure IoT Edge that reads the temperature from the HS18B2 temperature sensors.  The data is transmitted to an Azure IoT Hub instance and will be visualized by an Azure Time Series Insights resource.

## Documentation

This section contains more information on the steps that I've taken to get this working

- [Install IoT Edge Runtime on Raspberry Pi](./docs/iot-edge-on-pi.md)
- [Preparing the development environment](./docs/prepare-dev-environment.md)
- Deploy Azure resources via ARM script
- Creating IoT Edge modules
- [Deploying modules to the Raspberry Pi](./docs/deploying-modules.md)
- [Troubleshooting](./docs/troublehooting.md)

## Key take-aways

### Reading the temperature from the sensor devices

The DS18B2 temperature-sensors are connected to the Raspberry Pi and the measured temperature can be retrieved by reading a file that is present in the [`/sys/devices`](https://lwn.net/Articles/646617/) directory.

Since the IoT Edge module that is responsible for reading out those sensors is running in a container, it has offcourse no direct access to those resources on the host.  To enable the IoT Edge module to access that directory, a bind mount has been specified so that the IoT Edge module can read files from the `/sys/devices` directory.  The bind has been created by specifying it in the `createOptions` section of that module:

```json
"DS18B2TemperatureReader": {
  "version": "1.1",
  "type": "docker",
  "status": "running",
  "restartPolicy": "always",
  "settings": {
  "image": "${MODULES.DS18B2TemperatureReader}",
  "createOptions": {
    "HostConfig":{
      "Mounts": [
                  {
                    "Type": "bind",
                    "Target": "/w1devices",
                    "Source": "/sys/devices/w1_bus_master1"
                  }
                ]
    }
  }
}
```

Now, it is possible to access files that are present on the host in the /sys/devices directory by referring to them from the IoT Edge module via the /w1devices path, for instance:

```csharp
var sensorContent = File.ReadAllText("/w1devices/28-/w1_slave");
```
where `` is the sensor-id.

### Image format for Raspberry Pi

Since the Raspberry Pi 3 that I'm using is running a 32 bit version of Raspbian, the Docker images of the IoT Edge modules must be 32 bit ARM containers.
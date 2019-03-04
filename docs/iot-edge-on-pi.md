# How to install Azure IoT Edge on Raspberry Pi (Raspbian)

First of all, you must offcourse be able to ssh into the Raspberry Pi that is running a recent version of Raspbian.

## Install Moby

Since all modules that are hosted in IoT Edge are in fact containers, we'll need a container registry.
Execute the following commands on the commandline to install Moby.

```
curl -L https://aka.ms/moby-engine-armhf-latest -o moby_engine.deb && sudo dpkg -i ./moby_engine.deb

curl -L https://aka.ms/moby-cli-armhf-latest -o moby_cli.deb && sudo dpkg -i ./moby_cli.deb

sudo apt-get install -f
```

To verify if Moby is correctl installed, you can execute a command that returns the version of Docker
```
sudo docker version
```

## Install the Azure IoT Edge Runtime

The Azure IoT Edge Runtime can be installed by executing these commands:

```
curl -L https://aka.ms/libiothsm-std-linux-armhf-latest -o libiothsm-std.deb && sudo dpkg -i ./libiothsm-std.deb 
 
curl -L https://aka.ms/iotedged-linux-armhf-latest -o iotedge.deb && sudo dpkg -i ./iotedge.deb 
 
sudo apt-get install -f
```

Once this is done, you can find a `config.yaml` file in /etc/iodedge
In a next step, the connectionstring to the IoT Hub to which IoT Edge must communicate with must be specified in this file.

Microsoft documentation on how to install can be found [here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux-arm).
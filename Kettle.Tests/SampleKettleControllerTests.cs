using BDL.Kettle.Model;
using BDL.Kettle.Model.Inputs;
using BDL.Kettle.Model.Outputs;
using BDL.Kettle.Model.Sensors;
using BDL.Kettle.Model.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BDL.Kettle.Tests
{
    [TestClass]
    public class SampleKettleControllerTests
    {
        private Mock<ITemperatureSensor> temperatureSensorMock;
        private Mock<IWaterSensor> waterSensorMock;
        private Mock<IPowerLamp> powerLampMock;
        private Mock<IHeatingElement> heatingElementMock;
        private Mock<IPowerSwitch> powerSwitchMock;
        private KettleController kettleController;

        /// <summary>
        /// 1.	The kettle must have the following components: power switch, 
        /// power lamp, temperature sensor, water sensor and a heating element.
        /// </summary>
        [TestInitialize]
        public void KettleElements()
        {
            temperatureSensorMock = new Mock<ITemperatureSensor>();
            waterSensorMock = new Mock<IWaterSensor>();
            powerLampMock = new Mock<IPowerLamp>();
            heatingElementMock = new Mock<IHeatingElement>();
            powerSwitchMock = new Mock<IPowerSwitch>();
            
            kettleController = new KettleController(
                temperatureSensorMock.Object,
                waterSensorMock.Object,
                powerLampMock.Object,
                heatingElementMock.Object,
                powerSwitchMock.Object);
        }

        /// <summary>
        /// 2. When the power switch is switched on, the kettle controller 
        /// must activate the heating element and the lamp. 
        /// </summary>
        [TestMethod]
        public void PowerSwitchedOn_HeatingAndLampOn()
        {
            //initialize
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(true);
            powerSwitchMock.Setup(powerSwitch => powerSwitch.IsOn).Returns(false);
            //execute
            powerSwitchMock.Raise(powerSwitch => powerSwitch.SwitchedOn += null, new ValueChangedEventArgs<bool>(true));
            //check result
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOnAsync(),Times.Once);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOnAsync(), Times.Once);
        }

        /// <summary>
        /// 3. When the power switch is switched off, 
        /// all components must be deactivated.
        /// </summary>
        [TestMethod]
        public void PowerSwitchedOff_HeatingAndLampOff()
        {
            //initialize
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(true);
            powerSwitchMock.Setup(powerSwitch => powerSwitch.IsOn).Returns(true);
            //execute
            powerSwitchMock.Raise(powerSwitch => powerSwitch.SwitchedOff += null, new ValueChangedEventArgs<bool>(true));
            //check result
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOffAsync(), Times.Once);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOffAsync(), Times.Once);
        }

        /// <summary>
        /// 4. The power switch is a toggle switch and must be switched off 
        /// by the controller in any case where the controller decides to turn off the kettle.
        /// </summary>
        [TestMethod]
        public void TooglePowerSwitchOff()
        {
            //initialize
            powerSwitchMock.Setup(powerSwitch => powerSwitch.IsOn).Returns(true);
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(true);            
            temperatureSensorMock.Setup(sensor => sensor.CurrentValue).Returns(90);
            //execute
            powerSwitchMock.Raise(powerSwitch => powerSwitch.SwitchedOff += null, new ValueChangedEventArgs<bool>(true));
            //check result
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOffAsync(), Times.Once);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOffAsync(), Times.Once);
        }

        /// <summary>
        /// 5.	When the water temperature reaches 100 degrees Celsius, 
        /// the power must be switched off.
        /// </summary>
        [TestMethod]
        public void WhenTemperatureChangedTo100_KettleSwitchedOff()
        {
            //initialize
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(true);
            powerSwitchMock.Setup(powerSwitch => powerSwitch.IsOn).Returns(true);
            temperatureSensorMock.Setup(sensor => sensor.CurrentValue).Returns(90);
            //execute
            temperatureSensorMock.Raise(sensor => sensor.ValueChanged += null, new ValueChangedEventArgs<int>(101));
            //check result
            powerSwitchMock.Verify(powerSwitch => powerSwitch.SwitchOffAsync(), Times.Once);
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOffAsync(), Times.Once);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOffAsync(), Times.Once);
        }

        /// <summary>
        /// 6.The kettle controller should not attempt to heat if there is no water present part 1
        /// </summary>
        [TestMethod]
        public void WhenNoWater_PowerStayOff()
        {
            //When kettle has no water
            //initialize
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(false);
            //execute
            powerSwitchMock.Raise(powerSwitch => powerSwitch.SwitchedOn += null, new ValueChangedEventArgs<bool>(true));
            //check result
            powerSwitchMock.Verify(powerSwitch => powerSwitch.SwitchOnAsync(), Times.Never);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOnAsync(), Times.Never);
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOnAsync(), Times.Never);
        }

        /// <summary>
        /// 6. switch off if the water is removed during heating
        /// </summary>
        [TestMethod]
        public void WaterRemoved_PowerSwitchOff()
        {
            //When water is removed
            //initialize
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(true);
            //execute
            waterSensorMock.Raise(waterSensor => waterSensor.ValueChanged += null, new ValueChangedEventArgs<bool>(false));
            //check result
            powerSwitchMock.Verify(powerSwitch => powerSwitch.SwitchOffAsync(), Times.Once);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOffAsync(), Times.Once);
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOffAsync(), Times.Once);
        }

        /// <summary>
        /// 7.  When the heating element has a fault, it will raise HeatingElementException when switching on, 
        /// and the controller must ensure that all the components are off.
        /// </summary>
        [TestMethod]
        public void HeatingElementHasFault_KettleOff()
        {
            //initialize
            waterSensorMock.Setup(waterSensor => waterSensor.CurrentValue).Returns(true);
            heatingElementMock.Setup(heatingElement => heatingElement.SwitchOnAsync()).Throws(new HeatingElementException("HeatingElement is broken"));
            //execute
            powerSwitchMock.Raise(powerSwitch => powerSwitch.SwitchedOn += null, new ValueChangedEventArgs<bool>(true));
            //check result
            powerSwitchMock.Verify(powerSwitch => powerSwitch.SwitchOffAsync(), Times.Once);
            heatingElementMock.Verify(heatingElement => heatingElement.SwitchOffAsync(), Times.Once);
            powerLampMock.Verify(powerLamp => powerLamp.SwitchOffAsync(), Times.Once);
        }
        
    }
}

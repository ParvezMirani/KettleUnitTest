using BDL.Kettle.Model.Inputs;
using BDL.Kettle.Model.Outputs;
using BDL.Kettle.Model.Sensors;
using BDL.Kettle.Model.Workers;
using System;
using System.Threading.Tasks;

namespace BDL.Kettle
{
    public class KettleController
    {
        private readonly ITemperatureSensor temperatureSensor;
        private readonly IWaterSensor waterSensor;
        private readonly IPowerLamp powerLamp;
        private readonly IHeatingElement heatingElement;
        private readonly IPowerSwitch powerSwitch;

        public KettleController(ITemperatureSensor temperatureSensor, IWaterSensor waterSensor, IPowerLamp powerLamp, IHeatingElement heatingElement, IPowerSwitch powerSwitch)
        {
            this.temperatureSensor = temperatureSensor;
            this.temperatureSensor.ValueChanged += TemperatureSensor_ValueChanged;

            this.waterSensor = waterSensor;
            this.waterSensor.ValueChanged += WaterSensor_ValueChanged; ;

            this.powerLamp = powerLamp;
            this.heatingElement = heatingElement;

            this.powerSwitch = powerSwitch;
            this.powerSwitch.SwitchedOff += PowerSwitch_SwitchedOff; ;
            this.powerSwitch.SwitchedOn += PowerSwitch_SwitchedOn; ;
           
        }

        private async void PowerSwitch_SwitchedOn(object sender, EventArgs e)
        {
            try
            {
                //to heat if there is no water present
                if (!waterSensor.CurrentValue)
                {
                    return;
                }

                //activate the heating element and the lamp
                await TurnKettleElementsOn();
            }
            catch (HeatingElementException)
            {
                await TurnSwitchOff();
            }
        }

        private async void PowerSwitch_SwitchedOff(object sender, EventArgs e)
        {
            await TurnKettleElementsOff();//truns only elements off as switch is already off
        }
              
        private async void TemperatureSensor_ValueChanged(object sender, Model.ValueChangedEventArgs<int> valueChangedEventArg)
        {
            if (valueChangedEventArg.Value >= 100)
                await TurnSwitchOff();
        }

        private async void WaterSensor_ValueChanged(object sender, Model.ValueChangedEventArgs<bool> valueChangedEventArg)
        {
            if (!valueChangedEventArg.Value)
                await TurnSwitchOff();
            else
                await TurnSwitchOn();
        }

        //helper meathods
        private async Task TurnSwitchOn()
        {
            await powerSwitch.SwitchOnAsync();
            await TurnKettleElementsOn();
        }

        private async Task TurnSwitchOff()
        {
            await powerSwitch.SwitchOffAsync();
            await TurnKettleElementsOff();
        }

        private async Task TurnKettleElementsOn()
        {
            await heatingElement.SwitchOnAsync();
            await powerLamp.SwitchOnAsync();
        }

        private async Task TurnKettleElementsOff()
        {
            await heatingElement.SwitchOffAsync();
            await powerLamp.SwitchOffAsync();
        }
    }
}

# Kettle
C# Programing Test – Kettle Controller

## Scope
Implement controller software for a kettle.

The kettle controller is responsible for interfacing with the physical components that make up the kettle.   
These components are each represented by interfaces that provide the software controller access to their physical behaviours.
Other developers are responsible for implementing the other components of the kettle and these are not yet available, 
however the interfaces to these components have been agreed and are available in the supplied Model.

## Technical Requirements
1.	The kettle controller must be a C# class.
2.	The various kettle component interfaces and the Kettle.Model library must not be modified.
3.	You cannot add new component interfaces.
4.	The kettle controller must have a full complement of unit tests; 
the kettle components should be mocked to complete these tests.  
These mocked components may expose functionality beyond the scope of the component interfaces but only for the benefit of unit testing.
5.	The solution should be as simple as possible and only cover the below requirements. 
6.	A not implemented KettleController is available as a guidance. Feel free to change it entirely.
It is not needed to add any other dependencies to the controller.
7.	You can use any mock framework, libraries, nuget packages that you consider useful. 
8.	A basic unit test is provided as a guidance, using Moq nuget library, but it is not required to follow the same patterns, tools or libraries, or even using any libraries at all. You could create your own mock if you wish.
9.  The sample projects have been created with Visual Studio 2017 and .Net Core 2. Feel free to use a different version of Visual Studio and .NET.

## Functional Requirements
1.	The kettle must have the following components: power switch, power lamp, temperature sensor, water sensor and a heating element.
2.	When the power switch is switched on, the kettle controller must activate the heating element and the lamp. 
3.	When the power switch is switched off, all components must be deactivated.
4.	The power switch is a toggle switch and must be switched off by the controller in any case where the controller decides to turn off the kettle.
5.	When the water temperature reaches 100 degrees Celsius, the power must be switched off.
6.	The kettle controller should not attempt to heat if there is no water present and should switch off if the water is removed during heating.
7.  When the heating element has a fault, it will raise HeatingElementException when switching on, and the controller must ensure that all the components are off.
### Notes
- The temperature sensor reports in degrees Celsius.
- The switchable components are idempotent and can safely be called multiple times.
- The Kettle controller will always be initialised with all the components off


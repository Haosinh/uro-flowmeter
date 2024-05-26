//#ifndef connect_h
//#define connect_h
//
//#define I2C_SCL 9
//#define I2C_SDA 8
//#define BUZZER 38
//#define BTN_RESET 37
//#define HX711_DOUT 15
//#define HX711_SCK 16
//#define SENSOR_PROMIXY 7
//#define RGB_LED 6
//#define ADC_BATERRY 4
//#define CD42_KEY 5
//
//void setup() {
//  // initialize serial communication at 115200 bits per second:
//  Serial.begin(115200);
//
//  //set the resolution to 12 bits (0-4096)
//  analogReadResolution(12);
//}
//
//void loop() {
//  // read the analog / millivolts value for pin 2:
//  int analogValue = analogRead(ADC_BATERRY);
//  int analogValue2 = analogRead(SENSOR_PROMIXY);
//  // int analogVolts = analogReadMilliVolts(ADC_BATERRY);
//
//  // print out the values you read:
//  Serial.printf("ADC ADC_BATERRY value = %d\n", analogValue);
//  Serial.printf("ADC SENSOR_PROMIXY  value = %d\n", analogValue2);
//
//  delay(100);  // delay in between reads for clear read from serial
//}
//#endif

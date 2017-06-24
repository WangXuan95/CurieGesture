/*
 *  Author      : 灯灯
 *  HardWare    : Intel Curie
 *  Description : 校正加速度计零点值
 *  Usage       : 上传程序后，保证Curie平放，等待板载LED灯亮，即完成校正
 */

#include <CurieIMU.h>

// 板载LED灯控制
#define LED_PIN     13
#define LED_READY  { pinMode(LED_PIN, OUTPUT);digitalWrite(LED_PIN, LOW); }
#define LED_ON     digitalWrite(LED_PIN, HIGH)

void resetIMU(){
  CurieIMU.autoCalibrateAccelerometerOffset(X_AXIS, 0);
  CurieIMU.autoCalibrateAccelerometerOffset(Y_AXIS, 0);
  CurieIMU.autoCalibrateAccelerometerOffset(Z_AXIS, 1);
  CurieIMU.autoCalibrateGyroOffset();
}

void setup(){
  LED_READY;
  CurieIMU.begin();
  resetIMU();
  LED_ON;
}

void loop(){
}


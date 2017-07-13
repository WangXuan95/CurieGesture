/*
 *  Author      : 灯灯
 *  HardWare    : Intel Curie
 *  Description : Hand Gesture Training and classifing
 *  Notice      : 该程序可以完成手势的训练和识别，识别结果通过串口和蓝牙传输，并能把训练结果存入板载SPI flash，上电可加载以前识别的结果。
 *                程序复位后，若在SPIflash里找到了以前的训练数据，板载LED灯有5秒的点亮时间，
 *                这5秒内如果你没打开串口，程序会自动读取以前的训练数据，并开始识别。
 *                若5秒内你打开了串口，你可以选择新训练数据，或读取以前的数据，总之，请按串口的提示去做。
 *                若SPIflash里没有以前的训练数据，板载LED灯不会亮，等待用户打开串口，进行训练
 *                识别时，识别结果通过串口和蓝牙两种方式传输，你可以用我写的Gesture_Show_UWP(一个UWP程序)应用观察识别结果
 */

#include <CurieIMU.h>
#include <CuriePME.h>
#include <CurieBLE.h>
#include <SerialFlash.h>
#include "SaveAndLoadNeurons.h"
#include "DataPreProcessor.h"

// 单个手势的学习次数
#define LEARN_CNT   5

// 注意：
// 做所有手势前必须保持静止，手势做完也有保持静止，程序会自动从两个静止中提取动作数据。
// 不要连续做两个动作，动作中间一定要有至少半秒的停顿
// 一个动作不能太长也不能太短，控制在半秒到2秒之内
// 做动作的时候尽量保持101指向一个方向，不要翻转它
// 在此处添加更多的手势
char *gestures[] = {
  "Right shift and go back",        // 右划并返回原位
  "Left shift and go back",         // 左划并返回原位
  "Draw circle anticlockwise",      // 逆时针画圈
  "Draw circle clockwise",          // 顺时针画圈
};

dataPreProcessor processor;

const int GESTURE_CNT = ( sizeof(gestures) / sizeof(*gestures) ) ;


// 板载LED灯控制
// 当101识别到手势时，LED灯亮
#define LED_PIN     13
#define LED_READY  pinMode(LED_PIN, OUTPUT)
#define LED_ON     digitalWrite(LED_PIN, HIGH)
#define LED_OFF    digitalWrite(LED_PIN, LOW)

bool ask(const char* question){
  Serial.print(question);
  Serial.println(" (y|n)");
  int c;
  do{
    c = Serial.read();
  }while(c!='y' && c!='n');
  return c=='y';
}

BLEPeripheral blePeripheral;
BLEService S("180D");
BLECharacteristic C("2A37", BLENotify, 2);


void setup() {
  Serial.begin(115200);

  CurieIMU.begin();
  CurieIMU.setAccelerometerRange(5);

  blePeripheral.setLocalName("Curie");
  blePeripheral.setAdvertisedServiceUuid(S.uuid());
  blePeripheral.addAttribute(S);
  blePeripheral.addAttribute(C);
  blePeripheral.begin();

  CuriePME.begin();

  bool flashAvailable = SerialFlash.begin(FlashChipSelect);

  LED_READY;
  LED_OFF;
  
  if ( flashAvailable && SerialFlash.exists(FILE_NAME) ) {
      LED_ON;
      for(int i=0;;i++){
        if(i==50){
          restoreNetworkKnowledge();
          LED_OFF;
          return;
        }
        delay(100);
        if(Serial){
          LED_OFF;
          if( ask("Old trained data found! Do you want to load learned data?(y) or train new gestures?(n)") ){
            restoreNetworkKnowledge();
            Serial.println("Now do gestures. Arduino 101 will classify them.");
            return;
          }else{
            break;
          }
        }
      }
  }else{
    while(!Serial);
    Serial.println("Unable to access SPI Flash chip, or no old training data. You can only train new gestures");
  }
  
  
  Serial.println();

  // 开始进行训练
  for(int i=0; i<GESTURE_CNT; i++){
    Serial.print("gesture name: "); Serial.println(gestures[i]);
    ask("Press y or n to start training");
    // 倒计时过程中，用户把Arduino 101拿在手里，准备学习
    for(int i=5;i>0;i--){
      Serial.print(i);Serial.println("...");
      delay(1000);
    }
    for(int j=0; j<LEARN_CNT; j++){
      Serial.print(j+1); Serial.print("/"); Serial.print(LEARN_CNT); Serial.print("  Now do this gesture...");
      for(;;){
          processor.waitForNextGesture();
          LED_ON;
          if( processor.nextGesture() )
              break;
          LED_OFF;
          Serial.print("invalid gesture,try again...");
      }
      LED_OFF;
      CuriePME.learn(processor.data, 3*LLEN, i+1);
      Serial.println("OK!");
    }
    Serial.println("Done with this gesture!");
    Serial.println();
  }

  if( flashAvailable && ask("Do you want to save trained data? this may cover the old trained data.") ){
    saveNetworkKnowledge();
  }

  // 训练结束，在loop函数里进行手势识别
  Serial.println("Now do gestures. Arduino 101 will classify them.");
  Serial.println();
}



// 持续手势识别...
void loop(){
  static uint8_t data[] = {0,0};

  processor.waitForNextGesture();

  LED_ON;
  Serial.print("Start Matching...");
  data[1] = 0xfe;
  C.setValue(data,2);
  bool result = processor.nextGesture();
  LED_OFF;

  if( result ){
    int answer = CuriePME.classify(processor.data, 3*LLEN);
    if( answer == CuriePME.noMatch ){
      data[1] = 0;
      Serial.println("Unknown Gesture");
    }else{
      data[1] = (uint8_t)answer;
      Serial.println(gestures[answer-1]);
    }
  }else{
    data[1] = 0xff;
    Serial.println("Invalid Gesture");
  }

  C.setValue(data,2);
}

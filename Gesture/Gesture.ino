/*
 *  Author      : 灯灯
 *  HardWare    : Intel Curie
 *  Description : Hand Gesture Training and classifing
 *  Notice      : Use this sketch to train gestures or classify gestures.
 *                But the information will only be shown on UART
 *                to show classified result by Bluetooth LE, please upload Gesture_BLE.ino
 */

#include <CurieIMU.h>
#include <CuriePME.h>
#include <SerialFlash.h>

// 注意：
// 做所有手势前必须保持静止，手势做完也有保持静止，程序会自动从两个静止中提取动作数据。
// 不要连续做两个动作，动作中间一定要有至少半秒的停顿
// 一个动作不能太长也不能太短，控制在半秒到2秒之内
// 做动作的时候尽量保持101指向一个方向，不要翻转它
// 在此处添加更多的手势

char *gestures[] = {
  "Right shift and go back",        // 右划并返回原位
  "Left shift and go back",         // 左划并返回原位
  "Up shift and go back",
  "Down shift and go back",
  "Draw circle anticlockwise",      // 逆时针画圈
  "Draw circle clockwise",          // 顺时针画圈
};



const int GESTURE_CNT = ( sizeof(gestures) / sizeof(*gestures) ) ;

// 板载LED灯控制
// 当101识别到手势时，LED灯亮
#define LED_PIN     13
#define LED_READY  { pinMode(LED_PIN, OUTPUT);digitalWrite(LED_PIN, LOW); }
#define LED_ON     digitalWrite(LED_PIN, HIGH)
#define LED_OFF    digitalWrite(LED_PIN, LOW)



#define DEBUG_PRT(x) Serial.print(x)
#define DEBUG_CRLF   Serial.println()
#define ASSERT(x,msg) { if(!(x)) {Serial.print("*** Error! "); Serial.println(msg); } }



// 在WSIZE次采样内极差不超过THRESHODE，认为无手势
#define WSIZE       30
// 支持的最长手势片段
#define  LEN       230
// 支持的最短手势片段
#define MINLEN      30
// 时间归一化后的向量长度
#define  LLEN       30
// 认为无手势的门限
#define THRESHODE 2000
// 单个手势的学习次数
#define LEARN_CNT   20


const int FlashChipSelect = 21;

#define FILE_NAME "NeurData.dat"


// 数据预处理类
// 功能：手势片段截取、平滑滤波、时间归一化、幅值归一化、数据量精简、三轴数据拼接
// 预处理后的数据才能扔进神经网络
class dataPreProcessor{

private:

  int index;
  int x, y, z;
  int lx, ly, lz;
  int xWindow[WSIZE], yWindow[WSIZE], zWindow[WSIZE];
  int xBuffer[ LEN ], yBuffer[ LEN ], zBuffer[ LEN ];
  int xMax, xMin, yMax, yMin, zMax, zMin;
  int xAvg, yAvg, zAvg;

  // 读取三轴加速度计数据
  void readRawData(){
    do{
      CurieIMU.readAccelerometer(x, y, z);
    }while(x==lx && y==ly && z==lz);
    lx = x; ly = y; lz = z;
  }

  // 数据放入循环数组，并计算是否存在手势动作，存在则返回true，否则返回false
  bool push(){
    readRawData();

    xWindow[index] = x; yWindow[index] = y; zWindow[index] = z;
    index = (index+1) % WSIZE;

    xMax=xWindow[0], xMin=xWindow[0], yMax=yWindow[0], yMin=yWindow[0], zMax=zWindow[0], zMin=zWindow[0];

    xAvg = 0; yAvg = 0; zAvg = 0;

    for(int i=1; i<WSIZE; i++){
      xAvg += xWindow[i];
      yAvg += yWindow[i];
      zAvg += zWindow[i];
      if(xMax<xWindow[i]) xMax = xWindow[i];
      if(xMin>xWindow[i]) xMin = xWindow[i];
      if(yMax<yWindow[i]) yMax = yWindow[i];
      if(yMin>yWindow[i]) yMin = yWindow[i];
      if(zMax<zWindow[i]) zMax = zWindow[i];
      if(zMin>zWindow[i]) zMin = zWindow[i];
    }

    xAvg /= WSIZE;  yAvg /= WSIZE;  zAvg /= WSIZE;

    return ( (xMax-xMin) + (yMax-yMin) + (zMax-zMin) ) > THRESHODE ;
  }

  // 填满窗口
  void restart(){
    index = 0;
    for(uint16_t cnt=WSIZE; cnt>0; cnt--){
      push();
    }
  }

public:

  // 存放预处理结果，即神经网络用于训练的向量
  uint8_t data[LLEN*3];

  // 识别下一个动作，若动作不合法则返回false，合法则返回true，同时更新data向量。
  bool nextGesture(){
    restart();
    // 等待直到持续无动作
    while( push()==true );
    // 等待直到出现动作
    while( push()==false );

    int  xA = xAvg,  yA = yAvg,  zA = zAvg;
    int xMa=x, xMi=x, yMa=y, yMi=y, zMa=z, zMi=z;

    int len = 0;

    LED_ON;

    while( push()==true ){
      xBuffer[len] = x; yBuffer[len] = y; zBuffer[len] = z;
      if( (++len) >= LEN )
          return false;
      if(xMa<x) xMa = x;
      if(xMi>x) xMi = x;
      if(yMa<y) yMa = y;
      if(yMi>y) yMi = y;
      if(zMa<z) zMa = z;
      if(zMi>z) zMi = z;
    };

    LED_OFF;

    len -= WSIZE;
    if( len < MINLEN ) return false;

    int delta = max( max( (xMa-xMi) , (yMa-yMi) ) , (zMa-zMi) );

    xA = (xA+xAvg)/2;   yA = (yA+yAvg)/2;   zA = (zA+zAvg)/2;

    // 进行均值滤波、时间归一化、幅度归一化，得到预处理的最终步骤
    for(int i=0; i<LLEN; i++){
      int j = (i*len) / LLEN ;
      int k = ((1+i)*len) / LLEN ;
      data[i] = 128;  data[i+LLEN] = 128;  data[i+LLEN*2] = 128;
      for(int s=j; s<k; s++){
        data[i]        += ( (xBuffer[s]-xA) * 128 ) / delta / (k-j);
        data[i+LLEN]   += ( (yBuffer[s]-yA) * 128 ) / delta / (k-j);
        data[i+LLEN*2] += ( (zBuffer[s]-zA) * 128 ) / delta / (k-j);
      }
    }

    return true;
  }

} processor;


// 校正3轴加速度计值
void resetAccelerometer(){
  CurieIMU.autoCalibrateAccelerometerOffset(X_AXIS, 0);
  CurieIMU.autoCalibrateAccelerometerOffset(Y_AXIS, 0);
  CurieIMU.autoCalibrateAccelerometerOffset(Z_AXIS, 1);
}


bool ask(const char* question){
  Serial.print(question);
  Serial.println(" (y|n)");
  int c;
  do{
    c = Serial.read();
  }while(c!='y' && c!='n');
  return c=='y';
}


void setup() {
  LED_READY;

  Serial.begin(115200);
  while(!Serial);

  CurieIMU.begin();
  CurieIMU.setAccelerometerRange(5);
  Serial.println("Initialized CurieIMU!");

  // 校正3轴加速度计值（可选）
  // resetAccelerometer();

  CuriePME.begin();
  Serial.println("Initialized CuriePME!");

  bool flashAvailable = SerialFlash.begin(FlashChipSelect);

  if (!flashAvailable) {
    Serial.println("Unable to access SPI Flash chip. You can only train new gestures");
  }else{
    Serial.println("Initialized CurieSerialFlash!");
    if( SerialFlash.exists(FILE_NAME) && ask("Old trained data found! Do you want to load learned data?(y) or train new gestures?(n)") ){
      restoreNetworkKnowledge();
      Serial.println("\nNow do gestures. Arduino 101 will classify them.");
      Serial.println();
      return;
    }
  }
  
  Serial.println();

  // 开始进行训练
  for(int i=0; i<GESTURE_CNT; i++){
    ask("Press y or n to start training a new gesture");
    // 倒计时过程中，用户把Arduino 101拿在手里，准备学习
    Serial.println("Start Training a gesture after 5s, prepare to keep your Arduino 101 in static...");
    Serial.print("gesture name: "); Serial.println(gestures[i]);
    for(int i=5;i>0;i--){
      Serial.print(i);Serial.println("...");
      delay(1000);
    }
    for(int j=0; j<LEARN_CNT; j++){
      Serial.print(j+1); Serial.print("/"); Serial.print(LEARN_CNT); Serial.print("  Now do this gesture...");
      while(processor.nextGesture()==false){
        Serial.print("invalid gesture,try again...");
      };
      CuriePME.learn(processor.data, 3*LLEN, i+1);
      Serial.println("OK!");
    }
    Serial.println("Done with this gesture!");
    Serial.println();
  }

  if(!flashAvailable)
    flashAvailable = SerialFlash.begin(FlashChipSelect);

  if( flashAvailable && ask("Do you want to save trained data? this may cover the old trained data.") ){
    saveNetworkKnowledge();
  }

  // 训练结束，在loop函数里进行手势识别
  Serial.println("Now do gestures. Arduino 101 will classify them.");
  Serial.println();
}




// 持续手势识别...
void loop(){
  while(processor.nextGesture()==false);

  int answer = CuriePME.classify(processor.data, 3*LLEN);
  if( answer == CuriePME.noMatch ){
    Serial.println("Unknown Gesture");
  }else{
    Serial.println(gestures[answer-1]);
  }
}



void saveNetworkKnowledge(){
  const char *filename = "NeurData.dat";
  SerialFlashFile file;

  Intel_PMT::neuronData neuronData;
  uint32_t fileSize = 128 * sizeof(neuronData);

  Serial.print( "File Size to save is = ");
  Serial.print( fileSize );
  Serial.print("\n");

  create_if_not_exists( filename, fileSize );
  // Open the file and write test data
  file = SerialFlash.open(filename);
  file.erase();

  CuriePME.beginSaveMode();
  if (file) {
    // iterate over the network and save the data.
    while( uint16_t nCount = CuriePME.iterateNeuronsToSave(neuronData)) {
      if( nCount == 0x7FFF)
        break;

      Serial.print("Saving Neuron: ");
      Serial.print(nCount);
      Serial.print("\n");
      uint16_t neuronFields[4];

      neuronFields[0] = neuronData.context;
      neuronFields[1] = neuronData.influence;
      neuronFields[2] = neuronData.minInfluence;
      neuronFields[3] = neuronData.category;

      file.write( (void*) neuronFields, 8);
      file.write( (void*) neuronData.vector, 128 );
    }
  }

  CuriePME.endSaveMode();
  Serial.print("Knowledge Set Saved. \n");
}


bool create_if_not_exists(const char *filename, uint32_t fileSize){
  if (!SerialFlash.exists(filename)) {
    Serial.println("Creating file " + String(filename));
    return SerialFlash.createErasable(filename, fileSize);
  }

  Serial.println("File " + String(filename) + " already exists");
  return true;
}



void restoreNetworkKnowledge(){
  SerialFlashFile file;
  int32_t fileNeuronCount = 0;

  Intel_PMT::neuronData neuronData;

  // Open the file and write test data
  file = SerialFlash.open(FILE_NAME);

  CuriePME.beginRestoreMode();
  if (file) {
    // iterate over the network and save the data.
    while(1) {
      Serial.print("Reading Neuron: ");

      uint16_t neuronFields[4];
      file.read( (void*) neuronFields, 8);
      file.read( (void*) neuronData.vector, 128 );

      neuronData.context = neuronFields[0] ;
      neuronData.influence = neuronFields[1] ;
      neuronData.minInfluence = neuronFields[2] ;
      neuronData.category = neuronFields[3];

      if (neuronFields[0] == 0 || neuronFields[0] > 127)
        break;

      fileNeuronCount++;

      // this part just prints each neuron as it is restored,
      // so you can see what is happening.
      Serial.print(fileNeuronCount);
      Serial.print("\n");

      Serial.print( neuronFields[0] );
      Serial.print( "\t");
      Serial.print( neuronFields[1] );
      Serial.print( "\t");
      Serial.print( neuronFields[2] );
      Serial.print( "\t");
      Serial.print( neuronFields[3] );
      Serial.print( "\t");

      Serial.print( neuronData.vector[0] );
      Serial.print( "\t");
      Serial.print( neuronData.vector[1] );
      Serial.print( "\t");
      Serial.print( neuronData.vector[2] );

      Serial.print( "\n");
      CuriePME.iterateNeuronsToRestore( neuronData );
    }
  }

  CuriePME.endRestoreMode();
  Serial.print("Knowledge Set Restored. \n");
}

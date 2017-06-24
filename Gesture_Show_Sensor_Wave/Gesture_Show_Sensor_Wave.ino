/*
 *  Author      : 灯灯
 *  HardWare    : Intel Curie
 *  Description : 显示手势片段处理后的波形，
 *                用于展示加速度计手势预处理算法的效果
 *                进而证明了使用神经网络进行手势识别时可行的
 *  Usage       : 上传程序后，打开 Arduino IDE -> 工具 -> 串口绘图器，
 *                做一些手势观察波形
 */

#include <CurieIMU.h>

// 板载LED灯控制
// 当101识别到手势时，LED灯亮
#define LED_PIN     13
#define LED_READY  { pinMode(LED_PIN, OUTPUT);digitalWrite(LED_PIN, LOW); }
#define LED_ON     digitalWrite(LED_PIN, HIGH)
#define LED_OFF    digitalWrite(LED_PIN, LOW)




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



void setup() {
  LED_READY;

  Serial.begin(115200);
  while(!Serial);

  CurieIMU.begin();
  CurieIMU.setAccelerometerRange(5);

  // 校正3轴加速度计值（可选）
  // resetAccelerometer();
}




// 持续捕捉手势片段...
void loop(){
  processor.nextGesture();
  // 在“串口绘图器”上显示不同的手势经预处理后的数据
  for(int i=0; i<(LLEN*3); i++){
    Serial.println(processor.data[i]);
  }
}

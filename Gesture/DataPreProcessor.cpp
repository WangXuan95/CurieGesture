#include <CurieIMU.h>
#include "DataPreProcessor.h"

  // 读取三轴加速度计数据
void dataPreProcessor::readRawData(){
    int i = 0;
    do{
      CurieIMU.readAccelerometer(x, y, z);
      if((i++)>512) break;
    }while(x==lx && y==ly && z==lz);
    lx = x; ly = y; lz = z;
}

// 数据放入循环数组，并计算是否存在手势动作，存在则返回true，否则返回false
bool dataPreProcessor::push(){
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
void dataPreProcessor::restart(){
    index = 0;
    for(uint16_t cnt=WSIZE; cnt>0; cnt--){
      push();
    }
}


void dataPreProcessor::waitForNextGesture(){
    restart();
    // 等待直到持续无动作
    while( push()==true );
    // 等待直到出现动作
    while( push()==false );
}

// 识别下一个动作，若动作不合法则返回false，合法则返回true，同时更新data向量。
bool dataPreProcessor::nextGesture(){
    int  xA = xAvg,  yA = yAvg,  zA = zAvg;
    int xMa=x, xMi=x, yMa=y, yMi=y, zMa=z, zMi=z;

    int len = 0;

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

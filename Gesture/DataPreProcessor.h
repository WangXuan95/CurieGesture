// 数据预处理类
// 功能：手势片段截取、平滑滤波、时间归一化、幅值归一化、数据量精简、三轴数据拼接
// 预处理后的数据才能扔进神经网络

#ifndef _DATA_PRE_PROCESSOR_H_
#define _DATA_PRE_PROCESSOR_H_

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
  void readRawData();

  // 数据放入循环数组，并计算是否存在手势动作，存在则返回true，否则返回false
  bool push();

  // 填满窗口
  void restart();

public:

  // 存放预处理结果，即神经网络用于训练的向量
  uint8_t data[LLEN*3];

  void waitForNextGesture();

  // 识别下一个动作，若动作不合法则返回false，合法则返回true，同时更新data向量。
  bool nextGesture();

};

#endif


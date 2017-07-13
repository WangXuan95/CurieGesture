# Intel Curie (Arduino101) 手势识别项目

## Gesture

该程序可以完成手势的训练和识别，自动检测动作，识别结果通过串口和蓝牙传输，并能把训练结果存入板载SPI flash，上电可加载以前识别的结果。

程序复位后，若在SPIflash里找到了以前的训练数据，板载LED灯有5秒的点亮时间，
这5秒内如果你没打开串口，程序会自动读取以前的训练数据，并开始识别。
若5秒内你打开了串口，你可以选择新训练数据，或读取以前的数据，总之，请按串口的提示去做。
若SPIflash里没有以前的训练数据，板载LED灯不会亮，等待用户打开串口，进行训练

只要在你做动作时（无论训练与识别），板载LED灯都会亮

识别时，识别结果通过串口和蓝牙两种方式传输，你可以用我写的Gesture_Show_UWP(一个UWP程序)应用观察识别结果

## Gesture_Show_UWP

该UWP程序用于显示手势识别结果，请使用VS2015以上版本进行编译

![image](https://github.com/WangXuan95/CurieGesture/blob/master/uwp.png)

## Gesture_Show_Sensor_Wave

在串口绘图器里绘制预处理算法得到的波形，用于证明神经网络进行手势识别的可行性。

## resetAccelerometer

对加速度计进行零点校准。每个新的Curie芯片需要运行一次校准，才能进行手势训练和识别。


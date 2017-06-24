# Intel Curie (Arduino101) 手势识别项目

## Gesture

上传Gesture.ino到Curie，打开串口监视器，按串口提示对手势进行训练或识别。该程序不支持蓝牙。
训练后的数据可以存到板载SPI flash中，下次上电可以读取上次训练的数据直接用于识别。

## Gesture_BLE

该程序仅能用于识别，不能用于训练，手势识别结果通过蓝牙BLE传输，需配合UWP程序Gesture_Show_UWP使用。
上传Gesture_BLE.ino到Curie，用电脑蓝牙连接Curie，运行Gesture_Show_UWP查看手势识别结果

## Gesture_Show_UWP

该UWP程序用于显示手势识别结果，请使用VS2015以上版本进行编译

![image](https://github.com/WangXuan95/CurieGesture/blob/master/uwp.png)

## Gesture_Show_Sensor_Wave

在串口绘图器里绘制预处理算法得到的波形，用于证明神经网络进行手势识别的可行性。

## resetAccelerometer

对加速度计进行零点校准。每个新的Curie芯片需要运行一次校准，才能进行手势训练和识别。


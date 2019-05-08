# Kinect2.0数据说明
## 语音数据
文件 Audio.md

##彩色图像数据
文件 ColorBasics.md

## Kinetc 中有关位置点的说明
文件 KinectPoint.md

## 骨骼框架的有关数据
 骨骼节点的数据结构的说明在文件 BodyBasics.md 里


### Bones Hierarchy
骨骼层次结构从SpineBase作为根节点开始，一直延伸到肢体末端（头、指尖、脚）：

![](https://i.imgur.com/dTdr2AZ.png)

层级结构如下图所示：

![](https://i.imgur.com/oZvBeqq.png)

所有的关节姿态描述都以摄像机坐标系为参考，当人体站正朝向Kinect时SpineBase关节处的坐标系如下图所示。此时返回的关节四元数理论上应为(w,x,y,z)=(0,0,1,0)，对应的Z-Y-X欧拉角为(180°,0,180°)或(0,180°,0)

![](https://i.imgur.com/QE6Mb9U.png)

关节坐标系的Y轴沿着骨骼的方向，Z轴为骨骼转动轴（ Z-axis points to the direction that makes sense for the joint to rotate），X轴与Y轴和Z轴垂直，构成右手坐标系：

Bone direction(Y green) - always matches the skeleton.
Normal(Z blue) - joint roll, perpendicular to the bone
Binormal(X red) - perpendicular to the bone and norma
　　比如对于肘关节和膝关节来说，只有一个转动自由度，因此Z轴（平行于冠状轴）朝向身体的两侧，关节带动骨骼绕着Z轴旋转，如下图所示：

![](https://i.imgur.com/YAygE8t.png)
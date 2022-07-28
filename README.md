# DCMS
注意：Wesley 已经正式更名为 DCMS啦！

DCMS基于Saas的经销商快消解决方案，皆在满足区域营销管理业务快速变化需求，系统基于Docker + .Net core + Mysql Inner db cluster 的分布式微服务框架,提供高性能RPC远程服务调用，采用Zookeeper、Consul作为surging服务的注册中心，集成了哈希，随机，轮询，压力最小优先作为负载均衡的算法，RPC集成采用的是netty框架，采用异步传输，客户端APP 采用 Android Xamarin/ Xamarin.Forms 支持Android 5.0 以上 所有Android 最新版本。,DCMS 基于Saas的轻量级新零售快消CRM/ERP系统， 系统为经销商量身定制的全业务流程渠道分销管理系统（手机APP称为经销商管家），帮助经销商解决 业务信息化和数据化问题，提升管理效率，实现减员增效。

## 目标:

打通营销各层级，各环节之间的数据传输，实现区域内营销大数据挖掘分析：
1. 消费者回馈：直接和消费者直接接触，及时了解和掌握消费者对产品的意见
2. 终端管理：微信公众号互动让终端随时随地可以查询与经销商业务往来数据
3. 经销商管理：通过系统给经销商销售管理赋能，带动经销商全面信息化
4. 营销管理：实时获取经销商营销数据挖掘分析来指导生产、营销、营运根据目标市场运营状况的变化制定应对策略
5. 区域管理：一体化的系统管理减少因系统衔接对企业管理造成内耗

## 场景范围
通过使用本软件产品，能完成如下功能：
### 一、经销商管理
1. 基础数据维护
进行系统基础数据录入，方便系统接下来的业务操作和开展。
#### 场景：
1. 如何创建和维护员工档案
2. 如何创建和维护商品，品牌，以及商品的价格，促销活动，赠品额度等档案 
3. 如何创建和维护客户,客户等级，渠道，仓库等档案 
4. 如何创建和维护供应商档案 
5. 如何创建和维护财务科目，应收款期初等维护和导入 
6.如何进行系统全局基础和开单业务高级配置 

2. 采购进货
商品采购，采购退货，以及雪花预设商品的自动采购
#### 场景：
1. 如何采购开单，以及APP端的操作 
2. 采购退货怎么处理 
3. 销售管理
进行销售订单，退货订单，销售单，退货单，车辆对货单，收款对账单，仓库分拣，装车调度，订单转销售单等业务动作的发生与管理
#### 场景：
1. 如何销售开单，移动车销开单 
2. 销售订单出库后如何调度配送 
3. 销售出库时如何进行仓库拆零合并分拣货 
4. 销售出库时如何车辆调度，未调度的订单如果手动转销售出库 
5. 销售退货时，以及退货订单怎么处理 
6. 退货和拒签时如何车辆对货和二次调拨 
7. 经销商如何查询业务员对终端客户发生的收，退，欠，预收，费用情况 

4. 库存管理
进行调拨，盘点盈亏，成本调价，报损，盘点 (整仓)，盘点 (部分)，组合，拆分，等业务动作的发生与管理
#### 场景：
1. 如何指定供应商进行采购开单，以及APP端的操作 
2. 采购退货怎么处理 
3. 多仓库时，某一仓库库存告急时如何调拨库存 
4. 库存商品如何进行库存盘点 
5. 商品盘点盈/亏时如何进行库存调整 
6. 库存商品成本调整时如果进行成本调价 
7. 报损的商品如何计入费用 
8. 如何争对部分商品，或者指定的库存进行任务自动盘点 
9. 商品又组合需求时如果进行组合预占库存销售 
10. 按组合采购的商品库存如何进行拆分库存销售 

5. 客户业务管理
商品陈列，拜访，上报库存，评价客户，指导政策和资源分配
#### 场景：
1.业务员如何拜访签到以及库存上报 
2.配送人员如何进行终端签收 

6.绩效管理
业务员外勤和绩效
#### 场景：
1. 如何划分路线，并分配业务员 
2. 如何制定提成方案 
3. 如何查询统计汇总业务员提成金额 

7. 财务资金管理
进行收款，付款，预收款，预付款，费用支出，费用合同，其他收入，结转，账目，利润，来往帐目和资金的管理
#### 场景：
1.如何开设收款和付款单据，以及单据如何根据业务发生 
2.如何开设预收款和预付款单据，以及单据如何根据业务发生 
3.如何为客户手动开设收款单 
4.如何为供应商手动开设付款单 
5.如何为客户手动开设预收款单 
6.如何为供应商手动开设预付款单 
7.如何记账和月末结转 
8.如何核对余额和明细账 
9.如果核对和查看资产负债和利润 
10.供应商来往账目怎样核对 
11.供应商预付款如何对账 
12.客户来往账目怎样核对 
13.客户预收款如果对账 
14.如果查询预收款余额和预付款余额 

8. 业务与业绩报表
系统进销存各个模块以及财务等业务类报表的管理
#### 场景：
1. 如何查询销售明细表 
2. 如何查询销售汇总(按商品)  
3. 如何查询销售汇总(按客户)  
4. 如何查询销售汇总(按业务员)  
5. 如何查询销售汇总(客户/商品)  
6. 如何查询销售汇总(按仓库)  
7.如何查询销售汇总(仓库/商品)  
8. 如何查询销售汇总(按品牌)  
9. 如何查询订单明细订单汇总(按商品)  
10.如何查询费用合同明细表 
11.如何查询热销排行榜 
12.如何查询销量走势图 
13.如何查询销售商品成本利润报表 
14.如何查询采购明细表 
15.如何查询采购汇总（按商品） 
16.如何查询采购汇总（按供应商） 
17.如何查询库存表 
18.如何查询库存表(按仓库)  
19.如何查询库存变化表(汇总)  
20.如何查询库存变化表(按单据)  
21.如何查询门店库存上报表 
22.如何查询门店库存上报汇总表 
23.如何查询成本汇总表 
24.如何查询科目余额表 
25.如何查询资产负债表 
26.如何查询利润表 
27.如何查询明细分类账 
28.如何查询业务员业绩 
29.如何查询员工提成汇总表 
30.如何查询业务员拜访记录表 

9.预警决策
对客户流失，滞销，预警通知和消息的使用和管理
#### 场景：
1.如何知道库存是否滞销 
2.如何了解库存是否达到预警阀值 
3.如何知道库存商品是否临期预警 
4.如何了解业务员拜访是否达成 
5.如何知道客户采购销售的活跃度 
6.如何对客户价值进行分析 
7.如何知道客户是否流失 
8.如何知道商品在终端店铺的铺市率情况 

### 二、生产工厂/营销中心管理
1.经销商管理
经销商以及终端片区如何管理 
2.生产销售与营销决策分析
工厂出库和退货情况 
大区和各业务部销售情况 
经销商出入库情况 
经销商销售情况 
快速对账与核销 



## 客户业务需求

- 拜访客户:上门拜访时，如何准确无误的称呼业主名称？怎么样能够拉近与客户之间的距离？上次拜访时答应客户的事情，是否得以解决？如何维护客情？
- 线路规划：在手门店数量众多，分布位置杂短无序。每天应该如何开展拜访工作？如何才能合理分配拜访路线
- 订单获取：销售数据是否了然于心？门店库存是否得到合理的建议？
- 货品派送：终端销售点的订单是否得到快速的响应？送货员开始行动了吗？
- 工作汇报：每天拜访工作量巨大，工作汇报是否己是形式上的工作，没有实质性的内容？
- 客户开发：经销商老板安排的新门店开发任务执行到位了吗？
- 终端管理：网络划分、线路分配是否科学合理，门店信息杂乱无章，如何科学分配业代人员，合理规划线路？
- 订单汇总：经销商业代人员每天获取的订单是否得到了快速响应，并转入派送流程？
- 费用管理：经销商制定的促销政策是否有效的执行了？陈列费用的支出是否得到了回报？有没有直观的体现？
- 工作稽核：每天该安排业代人员拜访多少家门店？回访周期如何界定？ 经销商业代人员均属于移动办公状态，每天分配的工作是否有效的执行？
- 数据上报：经销商业代人员每天都有进行工作汇报吗？经销商业务员之间的工作是否得到有力的支持？
- 业务拓展：经销商老板安排的业务拓展工作为何总是很难达成？
- 销售管理：销售环节链条长，环节多，数据采集困难。
- 市场管理：终端体系多样化，监管乏力。
- 业务管理：外勤销售人员无法监管。
- 物流管理：仓库及配送无法合理分派，管理成本较高。
- 人力资源：人力资源无法合理分派，人力成本投入大，产出价值不高。
- 市场营销： 陈列、广告投入费用巨大，如何合理有效分配成为经销商心病。
- 工作效率：经销商应用单一系统较多，系统数据靠线下传递，工作效率不高。
- 费用核销：未与公司内部系统数据打通，全部线下沟通，下单、对账、核销效率较低，资金回报率不高。
        
## 功能:
  - 业务管理: 
   效率全面提升,业绩显著增长,移动开单,欠款提醒,门店拜访,奖励机制优化,实时了解经营状况，有效提醒与预测，让经销商决策更精准。

  - 业务场景: 
    临期品提醒,库存智能预警,库存查询，库存实时监控,补货/促销有据可依，支持车销，访单及订货会等多种销售模式以及快消退换货等复杂作业场景。

  - 模块化: 
    以模块化为思想，以业务领域为理念，以包管理(nuget\npm)为基础，充分解耦业务功能，使业务最大化的得到复用，极大减少重复开发时间，结合在线代码生成器，可轻松接入更多交易服务，供应链金融服务及物流服务等。

  - 客户管理: 
    客户价值智能分析,渠道/终端全掌控,客户流失预警,客户价值分析,客户拜访周期提醒,精确评价客户价值，有效指导渠道政策与资源分配，最大化客户留存与活跃，提升客户收入贡献。

  - 绩效管理: 
    通过数据量化考核,决策效率全面提升,可视化报表,销售数据分析,数据驱动决策;强大外勤功能与绩效分析，辅助经销商有效管理与激励，最大化业务员产出。

  - 开箱即用: 
    提供通用权限管理(Admin)、基础数据(Common)、任务调度(Quartz)、代码生成(CodeGenerator)等模块，开箱即用，让您专注于自己的业务开发。
    
## 客户端

|项目|库|描述|
|-------------|-----|-----------|
|DCMS|[GitHub](https://github.com/dorisoy/DCMS) | 经销商管家门户|
|DCMS.Client|[GitHub](https://github.com/dorisoy/DCMS.Client) | 基于Xamarin.Forms5.0+ 支持Android 5.0+/IOS|
|DCMS.Light|[GitHub](https://github.com/dorisoy/DCMS.Light) | 微信小程序客户端|


## APP截屏

<img align="left"  src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(8).jpg" width="260" vspace="20"/>
<img align="left"  src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(1).jpg" width="260" vspace="20"/>
<img src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(2).jpg" width="260" vspace="20"/>

##

<img  align="left"  src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(3).jpg" width="260" vspace="20"/>
<img  align="left"  src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(4).jpg" width="260" vspace="20"/>
<img  src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(5).jpg" width="260" vspace="20"/>

##

<img align="left"   src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(6).jpg" width="260" vspace="20"/>
<img align="left"   src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(9).jpg" width="260" vspace="20"/>
<img src="https://raw.githubusercontent.com/dorisoy/Wesley/main/d%20(7).jpg" width="260" vspace="20"/>

## 管理平台

<img align="left"  src="https://github.com/dorisoy/Wesley/blob/main/p%20(1).png?raw=true" width="260" vspace="20"/>
<img align="left"  src="https://github.com/dorisoy/Wesley/blob/main/p%20(2).png?raw=true" width="260" vspace="20"/>
<img src="https://github.com/dorisoy/Wesley/blob/main/p%20(3).png?raw=true" width="260" vspace="20"/>


## 开发环境

> IDE/开发语言
>
> > [Visual Studio 2019+](https://visualstudio.microsoft.com/zh-hans/downloads/)、
[Visual Studio Code](https://code.visualstudio.com/)、
[Android Studio](https://developer.android.google.cn/studio/)、
[C#](https://developer.android.google.cn/studio/)、
[Java](https://developer.android.google.cn/studio/)、
[Python](https://developer.android.google.cn/studio/)、
[Javas\Script](https://developer.android.google.cn/studio/)、
[TypeScript](https://developer.android.google.cn/studio/)、

> 微服务框架
>
> > [Zookeeper](https://dotnet.microsoft.com/download)、
[Docker](https://redis.io/)、
[gRPC](https://www.rabbitmq.com/)、
[Netty](https://www.mysql.com/)、
[Thrift](https://github.com/StackExchange/Dapper)、
[K8S](http://www.quartz-scheduler.org/)、
[Docker](https://serilog.net/)、
[Rancher](https://automapper.org/)、
[Consul](https://fluentvalidation.net)、
[Elasticsearch](https://fluentvalidation.net)、
[Skywalking](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

> 后端
>
> > [.Net Core 3.1](https://dotnet.microsoft.com/download)、
[Redis](https://redis.io/)、
[RabbitMQ](https://www.rabbitmq.com/)、
[MSSQL Server 2016+](https://www.mssql.com/)、
[MySQL8.0+](https://www.mysql.com/)、
[Dapper](https://github.com/StackExchange/Dapper)、
[Quartz](http://www.quartz-scheduler.org/)、
[Serilog](https://serilog.net/)、
[AutoMapper](https://automapper.org/)、
[FluentValidation](https://fluentvalidation.net)、
[Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)


> Web前端
>
> > [Node.js 10+](https://nodejs.org/en/)、[TypeScript 4.0 +](https://www.typescriptlang.org/)、[Vue.js 2.6+](https://cn.vuejs.org/)、[Vue CLI](https://cli.vuejs.org/zh/guide/)、[Vuex](https://vuex.vuejs.org/zh/)、[VueRouter](https://router.vuejs.org/zh/)、[Element-UI](https://element.eleme.cn/#/zh-CN/component/installation)

> Android/IOS 客户端
>
> > [Acr.UserDialogs](https://)、
[EasyNetQ](https://)、
[Microsoft.CognitiveServices.Speech](https://)、
[Microsoft.CSharp](https://)、
[NETStandard.Library](https://)、
[Newtonsoft.Json](https://)、
[Prism.Plugin.Popups](https://)、
[ReactiveUI.Fody](https://)、
[Shiny.Notifications](https://)、
[Shiny.Prism](https://)、
[SkiaSharp.Views.Forms](https://)、
[sqlite-net-pcl](https://)、
[System.Reactive](https://)、
[System.Reactive.Linq](https://)、
[System.ValueTuple](https://)、
[Xam.Plugin.Media](https://)、
[Xam.Plugin.SimpleAudioPlayer](https://)、
[Xam.Plugins.Forms.ImageCircle](https://)、
[Xam.Plugins.Settings](https://)、
[Xamarin.Essentials](https://)、
[Xamarin.Essentials.Interfaces](https://)、
[Xamarin.FFImageLoading.Forms](https://)、
[Xamarin.FFImageLoading.Transformations](https://)、
[Xamarin.Forms](https://)、
[Xamarin.Forms.PancakeView](https://)、
[ZXing.Net.Mobile](https://)、
[ZXing.Net.Mobile.Forms](https://)



## 销售模块

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 销售单据 		| √ |√|
| 退货订单 		| √ |√|
| 车辆对货单    | √ |×|
| 收款对账单 	| √ |√|
| 仓库分拣 		| √ |×| 
| 装车调度 		| √ |√|
| 订单转销售单 	| √ |×|
| 换货单 			| √ |√|
| 销售明细表 			| √ |√|
| 销售汇总(按商品) 	| √ |√|
| 销售汇总(按客户) 	| √ |√|
| 销售汇总(按业务员) 	| √ |√|
| 销售汇总(客户商品) 	| √ |√|
| 销售汇总(按仓库) 	| √ |√|
| 销售汇总(按品牌) 	| √ |√|
| 订单明细 				| √ |√|
| 订单汇总(按商品) 	| √ |√|
| 费用合同明细表 		| √ |×|
| 赠品汇总 				| √ |×|
| 销量走势图 			| √ |√|
| 销售商品成本利润 		| √ |×|

## 采购模块

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 采购单 		| √ |√|
| 采购退货单| √ |√|

## 仓储模块

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 调拨单| √ |√|
| 盘点盈亏单| √ |√|
| 成本调价单| √ |√|
| 报损单| √ |√|
| 盘点单(整仓)| √ |√|
| 盘点单(部分)| √ |√|
| 组合单| √ |√|
| 拆分单| √ |√|
| 库存表| √ |√|
| 库存变化表(汇总)| √ |√|
| 库存变化表(按单据)| √ |√|
| 门店库存上报表| √ |√|
| 门店库存上报汇总表| √ |√|
| 调拨明细表| √ |√|
| 调拨汇总表-按商品| √ |√|
| 成本汇总表| √ |√|
| 库存滞销报表| √ |√|
| 库存预警表| √ |√|
| 临期预警表| √ |√|

## 财务模块

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 收款单| √ |√|
| 付款单| √ |√|
| 预收款单| √ |√|
| 预付款单| √ |√|
| 费用支出| √ |√|
| 费用合同| √ |√|
| 财务收入| √ |√|
| 期末结转| √ |√|
| 录入凭证| √ |√|
| 科目余额表| √ |√|
| 资产负债表| √ |√|
| 利润表| √ |√|
| 明细分类账| √ |√|

## 档案模块

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 商品档案| √ |√|
| 品牌档案| √ |√|
| 单位档案| √ |√|
| 统计类别| √ |√|
| 价格方案| √ |√|
| 赠品额度| √ |√|
| 促销活动| √ |√|
| 上次售价| √ |√|
| 终端档案| √ |√|
| 仓库档案| √ |√|
| 渠道档案| √ |√|
| 终端等级| √ |√|
| 供应商档案| √ |√|
| 应收款期初| √ |√|
| 提成方案| √ |√|
| 员工提成| √ |√|
| 员工档案| √ |√|
| 操作员角色| √ |√|
| 制定线路| √ |√|
| 分配线路| √ |√|

## 报表模块

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 客户往来账| √ |√|
| 客户应收款| √ |√|
| 供应商往来账| √ |√|
| 供应商应付款| √ |√|
| 预收款余额| √ |√|
| 预付款余额| √ |√|
| 业务员业绩| √ |√|
| 员工提成汇总表| √ |√|
| 业务员拜访记录| √ |√|
| 拜访达成表| √ |√|
| 业务员外勤轨迹| √ |√|
| 客户活跃度| √ |√|
| 客户价值分析| √ |√|
| 客户流失预警| √ |√|
| 铺市率报表| √ |√|

## 系统配置

| 功能 | PC | APP |
| :----- | :----- | :----- |
| 系统设置| √ |√|
| 库存预警设置| √ |√|
| APP打印设置| √ |√|
| 电脑打印设置| √ |√|
| 会计科目| √ |√|
| 公司设置| √ |√|
| 价格体系设置| √ |√|
| 打印模板| √ |√|
| 备注设置| √ |√|
| 商品设置| √ |√|
| 财务设置| √ |√|


## 微信扫码交流

![](https://github.com/dorisoy/Wesley/blob/main/weixing.png?raw=true)

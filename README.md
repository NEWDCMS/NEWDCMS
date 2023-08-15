# NEWDCMS
注意：Wesley 已经正式更名为 NEWDCMS啦！

NEWDCMS基于Saas的经销商快消解决方案，皆在满足区域营销管理业务快速变化需求，系统基于Docker + .Net core + Mysql Inner db cluster 的分布式微服务框架,提供高性能RPC远程服务调用，采用Zookeeper、Consul作为surging服务的注册中心，集成了哈希，随机，轮询，压力最小优先作为负载均衡的算法，RPC集成采用的是netty框架，采用异步传输，客户端APP 采用 Android Xamarin/ Xamarin.Forms 支持Android 5.0 以上 所有Android 最新版本。基于Saas的轻量级新零售快消CRM/ERP系统， 系统为经销商量身定制的全业务流程渠道分销管理系统（手机APP称为经销商管家），帮助经销商解决 业务信息化和数据化问题，提升管理效率，实现减员增效。

## 目标:

打通营销各层级，各环节之间的数据传输，实现区域内营销大数据挖掘分析：
1. 消费者回馈：直接和消费者直接接触，及时了解和掌握消费者对产品的意见
2. 终端管理：微信公众号互动让终端随时随地可以查询与经销商业务往来数据
3. 经销商管理：通过系统给经销商销售管理赋能，带动经销商全面信息化
4. 营销管理：实时获取经销商营销数据挖掘分析来指导生产、营销、营运根据目标市场运营状况的变化制定应对策略
5. 区域管理：一体化的系统管理减少因系统衔接对企业管理造成内耗


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
|NEWDCMS|[GitHub](https://github.com/dorisoy/NEWDCMS) | 经销商管家门户|
|NEWDCMS.Client|[GitHub](https://github.com/dorisoy/NEWDCMS.Client) | 基于Xamarin.Forms5.0+ 支持Android 5.0+/IOS|
|NEWDCMS.Light|[GitHub](https://github.com/dorisoy/NEWDCMS.Light) | 微信小程序客户端|
|NEWDCMS.Blazor|[GitHub](https://github.com/dorisoy/NEWDCMS.Blazor) | 基于Blazor实现的前后端分离版本|

## Blazor 版

<img align="left"  src="https://raw.githubusercontent.com/dorisoy/DCMS.Blazor/main/1.png" width="260" vspace="20"/>
<img align="left"  src="https://raw.githubusercontent.com/dorisoy/DCMS.Blazor/main/2.png" width="260" vspace="20"/>
<img src="https://raw.githubusercontent.com/dorisoy/DCMS.Blazor/main/3.png" width="260" vspace="20"/>

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

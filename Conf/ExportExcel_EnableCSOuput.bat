:: 关闭命令输出打印
@echo off
:: 清除命令行界面内容
CLS
:: 输出打印信息
echo "Start Export Excel To Bytes"
:: 启动导表工具
start XbufferExcelToData.exe false
:: 退出命令界面 0代表正常退出
EXIT 0
# 常用命令

## Windows 系统命令
```powershell
# 文件操作
dir                    # 列出目录 (等同于 ls)
type file.txt          # 查看文件内容 (等同于 cat)
copy / xcopy           # 复制文件
move                   # 移动文件
del                    # 删除文件
mkdir                  # 创建目录

# 搜索
findstr "pattern" *.cs  # 搜索文本 (等同于 grep)
where /r . *.cs         # 查找文件 (等同于 find)
```

## Git 命令
```bash
git status              # 查看状态
git add .               # 暂存所有
git commit -m "msg"     # 提交
git push                # 推送
git pull                # 拉取
git log --oneline -10   # 查看最近提交
```

## Unity 相关
- **打开项目**: 通过 Unity Hub 或直接打开 .sln 文件
- **编辑器**: 使用 Rider 或 VS Code 打开 `Silent-Planet.sln`
- **Input Actions**: 编辑 `Assets/Settings/InputSystem_Actions.inputactions`

## 项目特定
- **文档目录**: `Documents/`
- **脚本目录**: `Assets/_Scripts/`
- **配置目录**: `Assets/_Scripts/Settings/`

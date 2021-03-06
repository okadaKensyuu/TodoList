param($ToolsPath, $ConfigurationName, $SolutionDir, $ProjectName, $HostedProjectDir)

echo "==================== variables ===================="
echo "ToolsPath:         $ToolsPath"
echo "ConfigurationName: $ConfigurationName"
echo "SolutionDir:       $SolutionDir"
echo "ProjectName:       $ProjectName"
echo "HostedProjectDir:  $HostedProjectDir"
echo "==================================================="

echo "start proc..."

cd $ToolsPath

$SolutionDir = $SolutionDir.TrimEnd('\')
.\Gbook.Base.Configuration.Tool.exe $ConfigurationName $SolutionDir $ProjectName $HostedProjectDir

$ret = $lastexitcode
if ($ret -ne 0) {
  exit $ret
}

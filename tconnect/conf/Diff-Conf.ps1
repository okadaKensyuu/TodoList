mkdir -Force diff
..\tool\xmldiff\XmlDiff.exe OldConf\TConnect.Web.Preproduction.config Gbook.Base.Configuration.Tool\reserved\Web.Preproduction.config > diff\Diff-WebConfig-Preproduction.html
..\tool\xmldiff\XmlDiff.exe OldConf\TConnect.Web.Commercial.config Gbook.Base.Configuration.Tool\reserved\Web.Commercial.config > diff\Diff-WebConfig-Commercial.html

..\tool\xmldiff\XmlDiff.exe OldConf\ServiceConfiguration.Preproduction.cscfg Gbook.Base.Configuration.Tool\reserved\ServiceConfiguration.Cloud.Preproduction.cscfg > diff\Diff-ServiceConfiguration-Preproduction.html
..\tool\xmldiff\XmlDiff.exe OldConf\ServiceConfiguration.Commercial.cscfg Gbook.Base.Configuration.Tool\reserved\ServiceConfiguration.Cloud.Commercial.cscfg > diff\Diff-ServiceConfiguration-Commercial.html

..\tool\xmldiff\XmlDiff.exe OldConf\ServiceDefinition.Preproduction.csdef Gbook.Base.Configuration.Tool\reserved\ServiceDefinition.Preproduction.csdef > diff\Diff-ServiceDefinition-Preproduction.html
..\tool\xmldiff\XmlDiff.exe OldConf\ServiceDefinition.Commercial.csdef Gbook.Base.Configuration.Tool\reserved\ServiceDefinition.Commercial.csdef > diff\Diff-ServiceDefinition-Commercial.html

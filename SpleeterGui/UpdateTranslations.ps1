# Invoke this script fron the Nuget package manager console.  CD into the directory of this script before invoking.

# gettext tools don't setup their path correctly yet, so here is a work-around
$env:Path += ";..\packages\Gettext.Tools.0.19.8.1\tools\bin"

# Extract msgids from xaml files in project into pot file.  If you installed NGettext.Wpf via nuget you can source like so:
. XGetText-Xaml.ps1
# instead of the following
#   . ../XGetText.Xaml/XGetText-Xaml.ps1
XGetText-Xaml -o obj/xamlmessages.pot -k Gettext,GettextFormatConverter @(Get-ChildItem -Recurse -File -Filter *.xaml | Where { $_.FullName -NotLike '*\obj\*' } | ForEach-Object { $_.FullName })

Get-ChildItem -Recurse -File -Filter *.cs | Where { $_.FullName -NotLike '*\obj\*' } | ForEach-Object { $_.FullName } | Out-File -Encoding ascii "obj\csharpfiles"

# Extract msgids from cs files in project into pot file
xgettext.exe --force-po --from-code UTF-8 --language=c# -o obj/csmessages.pot -k_ -kNoop:1g -kEnumMsgId:1g --keyword=Catalog.GetString --keyword="PluralGettext:2,3" --files-from=obj\csharpfiles

# Merge two pot files into one
msgcat.exe --use-first -o obj/result.pot obj/csmessages.pot obj/xamlmessages.pot

# Update po files with most recent msgids
$locales = @("zh_CN")
$poFiles = $($locales | ForEach-Object { "locale/" + $_ + "/message.po" })

$poFiles | ForEach-Object {
	msgmerge.exe --sort-output --update $_ obj/result.pot 2> $null
}

echo "Po files updated with current msgIds: " $poFiles
echo "You may now edit these files with PoEdit (https://poedit.net/)"

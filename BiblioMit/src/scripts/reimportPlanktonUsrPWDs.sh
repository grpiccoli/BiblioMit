sqlcmd="/opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-48C10787-909E-4820-8017-89D16304F692 -C -Q"
while read p; do
	id=$(echo $p | cut -f1 -d'\t')
	pass=$(echo $p | cut -f1 -d'\t')
	cmd="$sqlcmd "'"'"UPDATE dbo.PlanktonUsers SET Password = '$pass' WHERE Id = $id"'"'
	echo $cmd
	eval "$cmd"
done < PlanktonUsers.tsv
cd /var/www/bibliomit
systemctl stop kestrel-bibliomit.service
dotnet BiblioMit.dll -- plankton 2>&1 > planktonUpdate.out &
pid=$_
sleep 4h && kill $pid
systemctl start kestrel-bibliomit.service
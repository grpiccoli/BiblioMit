####sqlcmd
sqlcmd="/opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U SA -P JGdtaStFe7LXf4A3 -d aspnet-BiblioMit-48C10787-909E-4820-8017-89D16304F692 -C -Q"
#table / file names
tables=(
'PlanktonAssays'
'Phytoplanktons'
#'PlanktonUsers'
#'Analists'
#'Emails'
#'Laboratories'
#'Phones'
#'PhylogeneticGroups'
#'GenusPhytoplanktons'
'PlanktonAssayEmails'
#'SamplingEntities'
#'SpeciesPhytoplanktons'
#'Stations'
)
# DELETE TABLES
arg=$(printf "DELETE FROM dbo.%s;" "${tables[@]}")
cmd="$sqlcmd '$arg'"
echo $cmd
eval "$cmd"
# Copy Files to /tmp folder
cmd=$(printf "cp /root/BiblioMit/BiblioMit/Data/Environmental/%s.tsv /tmp;" "${tables[@]}")
echo $cmd
eval "$cmd"
# BULK INSERT FILES
arg=$(echo "${tables[@]}" | tr ' ' '\n' | awk '{printf "BULK INSERT dbo.%s FROM \x27/tmp/%s.tsv\x27;", $1, $1 }')
cmd="$sqlcmd "'"'"$arg"'"'
echo $cmd
eval "$cmd"
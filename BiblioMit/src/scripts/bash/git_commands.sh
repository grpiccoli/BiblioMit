#remove all commits
git update-ref -d HEAD

#log all commits
git log --oneline

#delete last commit and keep changes
git reset --soft HEAD~1

#delete last commit and remove changes
git reset --hard HEAD~1
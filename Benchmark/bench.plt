set title "Operation of 1000,000 numbers in a POBTree"

set terminal svg
set out "bench.svg"

set xl "threads"
set yl "time(sec)"
set xrange [0:]
set yrange [0:]

plot "insert.dat" title "insert" with lines, "find.dat" title "find" with lines, "delete.dat" title "delete" with lines

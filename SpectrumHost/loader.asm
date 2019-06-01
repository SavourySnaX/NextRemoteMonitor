; vim:  autoindent noexpandtab tabstop=4 shiftwidth=4

; Simple crude loader to stick the monitor/remote down below

	include		"next.defs"
	include		"monitor.defs"



	org		$c000

start:
	di
	NextReg	ZXNR_MMU0,MONITOR_BANK
	ld		de,0
	ld		hl,monitor
	ld		bc,monitor_len
	ldir
	rst		$08



monitor:
	incbin		"monitor.bin"

monitor_len		equ		$-monitor

	end		$C000


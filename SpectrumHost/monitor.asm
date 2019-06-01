; vim:  autoindent noexpandtab tabstop=4 shiftwidth=4
;

;
; Rough first plan is as follows :
;
;  Monitor/remote should fit inside a single Page ($2000 bytes)
;  Initial version will sit the page in the first bank,
;   so RST 00 can be used as a breakpoint
;
; Not concerned with debugging code i don't develop, so this is fine for now
;  
; LAYOUT (Monitor Mode):
;	MMU0 Monitor/Remote - Currently fixed
;	MMU7 Used for transfers - only temporarily paged when needed
;

; Command Formats for Monitor
;
;  Listed here in terms of Spectrum Next view of things (e.g. download means the next is recieving data)
;
; * - Implemented
;
;   Initial Byte    |   Description	        |*|   Params
;	---------------------------------------------------------------------
;       00          | Exit                  |*| 	
;       01          | Download Data Banked  |*|  BB 	- Byte - Destination Bank (Next MMU style)
;                   |                       | |  LLHH - Word - Destination Address (should be 0-$1fff)
;                   |                       | |  LLHH - Word - Length (limit to 2000 max, since we are using a bank)
;       02          | Upload Data Banked    |*|  BB 	- Byte - Destination Bank (Next MMU style)
;                   |                       | |  LLHH - Word - Destination Address (should be 0-$1fff)
;                   |                       | |  LLHH - Word - Length (limit to 2000 max, since we are using a bank)
;       03          | Set next register     | |  RR   - Byte - next register to set
;                   |                       | |  VV   - Byte - value to set
;       04          | Get next register     | |  RR   - Byte - next register to query
;       05          | Set breakpoint        | |  NN   - Byte - breakpoint number (0-63)
;                   |                       | |  BB   - Byte - Bank
;                   |                       | |  LLHH - Word - Offset to place breakpoint (0-$1FFF)
;       06          | Execute               | |  LLHH - Word - Address to start execution at (0-$FFFF)
;
;

	include		"next.defs"
	include		"monitor.defs"

	org		$0000

rst00
	di								; 1 byte
	NextReg	ZXNR_MMU0,MONITOR_BANK	; 4 bytes
	jp		Breakpoint				; 3 bytes
rst08
	jp		MonitorStart			; A should contain return bank, stack contains return address

	org		$0038
rst38
	ei
	reti							; .. incase interrupted while paged... (e.g. doing a delay)


ExitBank:
	db		0
ExitAddress:
	dw		0

MonitorStart:

	ld		(ExitBank),a
	pop		hl
	ld		(ExitAddress),hl

	ld		hl, MonitorHandshake
	call	Rem_Initialise			; Setup the WIFI into raw state

	call	Process					; Run the monitor, note if Exit command recieved, we will return

	; TODO recover - for now crash on purpose
	ld		hl,(ExitAddress)
	push	hl

_rebootme
	jr	_rebootme

MonitorHandshake:
	db		"MON!",0

Process:
	call	Rem_GetRawByte

	cp		2
	jr		z,_SendData
	cp		1
	jr		z,_RecvData
	cp		0
	jr		nz,Process

_Exit:

	jp		Rem_Close

_RecvData:
	; Record current MMU7 page
	ld		bc,ZXN_REG_NUM
	ld		a,ZXNR_MMU7
	out		(c),a
	ld		bc,ZXN_REG_DATA
	in		a,(c)
	push	af

	call	Rem_GetRawByte	; Get Bank
	NextReg	ZXNR_MMU7,a
	call	Rem_GetRawWord	; Get offset
	ld		a,d
	or		$E0				; Force $E000+offset
	ld		h,a
	ld		l,e
	call	Rem_GetRawWord
	ld		b,d
	ld		c,e

_RecvLoop:
	push	bc
	call	Rem_GetRawByte
	pop		bc
	ld		(hl),a
	inc		hl
	dec		bc
	ld		a,b
	or		c
	jr		nz,_RecvLoop

	; Restore MMU7
	pop		af
	NextReg	ZXNR_MMU7,a
	
	jr		Process

_SendData:
	; Record current MMU7 page
	ld		bc,ZXN_REG_NUM
	ld		a,ZXNR_MMU7
	out		(c),a
	ld		bc,ZXN_REG_DATA
	in		a,(c)
	push	af

	call	Rem_GetRawByte	; Get Bank 
	NextReg	ZXNR_MMU7,a
	call	Rem_GetRawWord	; Get Addr
	ld		a,d
	or		$E0				; Force $E000+offset
	ld		h,a
	ld		l,e
	call	Rem_GetRawWord
	ld		b,d
	ld		c,e

_SendLoop:
	push	bc
	ld		a,(hl)
	inc		hl
	call	Rem_SendRawByte
	pop		bc
	dec		bc
	ld		a,b
	or		c
	jr		nz,_SendLoop

	; Restore MMU7
	pop		af
	NextReg	ZXNR_MMU7,a
	
	jr		Process



Breakpoint:						; TODO needs to grab registers, recover overwritten byte etc.
	ret

	include	"lib_remote.asm"

	end

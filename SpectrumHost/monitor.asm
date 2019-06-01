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
;	MMU0 Monitor/Remote
;	MMU7 Used for transfers
;

; Command Formats for Monitor
;
;  Listed here in terms of Spectrum Next view of things (e.g. download means the next is recieving data)
;
;
;   Initial Byte    |   Description	        |   Params
;	---------------------------------------------------------------------
;       00          | Exit                  | 	
;       01          | Download Data Banked  |  BB 	- Byte - Destination Bank (Next MMU style)
;                   |                       |  LLHH - Word - Destination Address (should be 0-$1fff)
;                   |                       |  LLHH - Word - Length (limit to 2000 max, since we are using a bank
;       02          | Upload Data Banked    |  BB 	- Byte - Destination Bank (Next MMU style)
;                   |                       |  LLHH - Word - Destination Address (should be 0-$1fff)
;                   |                       |  LLHH - Word - Length (limit to 2000 max, since we are using a bank
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

	; TODO recover - for now crash horribly probably
	ld		hl,(ExitAddress)
	push	hl
	ret

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
	call	Rem_GetRawByte	; Get Bank - discard for now
	call	Rem_GetRawWord	; Get Addr
	ld		h,d
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
	
	jr		Process

_SendData:
	call	Rem_GetRawByte	; Get Bank - discard for now
	call	Rem_GetRawWord	; Get Addr
	ld		h,d
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

	jr		Process



Breakpoint:						; TODO needs to grab registers, recover overwritten byte etc.
	ret

	include	"lib_remote.asm"

	end

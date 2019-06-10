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
;       01          | Download Data Banked  |*|  BB   - Byte - Destination Bank (Next MMU style)
;                   |                       | |  LLHH - Word - Destination Address (should be 0-$1fff)
;                   |                       | |  LLHH - Word - Length (limit to 2000 max, since we are using a bank)
;       02          | Upload Data Banked    |*|  BB   - Byte - Destination Bank (Next MMU style)
;                   |                       | |  LLHH - Word - Destination Address (should be 0-$1fff)
;                   |                       | |  LLHH - Word - Length (limit to 2000 max, since we are using a bank)
;       03          | Set next register     |*|  RR   - Byte - next register to set
;                   |                       | |  VV   - Byte - value to set
;       04          | Get next register     |*|  RR   - Byte - next register to query
;       05          | Set breakpoint        |*|  BB   - Byte - Bank
;                   |                       | |  LLHH - Word - Offset to place breakpoint (0-$1FFF)
;                   |                       | |  NN   - Byte - breakpoint number (0-63)
;       06          | Execute               |*|  LLHH - Word - Address to start execution at (0-$FFFF)
;       07          | Resume                |*|
;       08          | Get State             |*|  (see SetState for order registers are sent - same as recieved)
;       09          | Set State             |*|  LLHH - AF
;                   |                       | |  LLHH - BC
;                   |                       | |  LLHH - DE
;                   |                       | |  LLHH - HL
;                   |                       | |  LLHH - SP
;                   |                       | |  LLHH - PC
;                   |                       | |  LLHH - IX
;                   |                       | |  LLHH - IY
;                   |                       | |  LLHH - AF'
;                   |                       | |  LLHH - BC'
;                   |                       | |  LLHH - DE'
;                   |                       | |  LLHH - HL'
;                   |                       | |  LLHH - IR   - The value of R is questionable here since the monitor will change it
;                   |                       | |  LL00 - IFF2 - bit 2 interrupts enabled/disabled (will currently always be disabled when reading)
;       0A          | Set io port           |*|  LLHH - Port - port to set
;                   |                       | |  VV   - Byte - value to set
;       0B          | Get io port           |*|  LLHH - Port - port whose value to fetch

	include		"next.defs"
	include		"monitor.defs"

	org		$0000

rst00:
	di								; 1 byte
	NextReg	ZXNR_MMU0,MONITOR_BANK	; 4 bytes
	jp		Breakpoint				; 3 bytes
rst08:
	jp		MonitorStart			; A should contain return bank, stack contains return address
	org		$0038
rst38:
	ei
	reti							; .. incase interrupted while paged... (e.g. doing a delay)

	org		$0066
	NextReg ZXNR_MMU0, 8
	retn

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

	jp	CRASH

MonitorHandshake:
	db		"MON!",0

// Fine for upto 128 commands
Commands:
	dw		RecvData			; Starts at offset 1 !
	dw		SendData
	dw		SetNextReg
	dw		GetNextReg
	dw		SetBreakpoint
	dw		Execute
	dw		Resume
	dw		GetState
	dw		SetState
	dw		SetIOPort
	dw		GetIOPort

NumCommands	EQU	(($-Commands) /2) 

Process:
	call	Rem_GetRawByte

	and		a
	jp		z,Rem_Close

	dec		a

	cp		NumCommands
	jp		nc,Rem_Close

	add		a,a
	ld		hl,Commands
	add		hl,a
	ld		a,(hl)
	inc		hl
	ld		h,(hl)
	ld		l,a
	jp		(hl)

RecvData:
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

SendData:
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
	
	jp		Process

SetNextReg:
	call	Rem_GetRawByte	; Get register number
	ld		bc,ZXN_REG_NUM
	out		(c),a
	call	Rem_GetRawByte	; Get value
	ld		bc,ZXN_REG_DATA
	out		(c),a

	jp		Process

GetNextReg:
	call	Rem_GetRawByte	; Get register number
	ld		bc,ZXN_REG_NUM
	out		(c),a
	ld		bc,ZXN_REG_DATA
	in		a,(c)
	call	Rem_SendRawByte

	jp		Process

SetBreakpoint:
	; Record current MMU7 page
	ld		bc,ZXN_REG_NUM
	ld		a,ZXNR_MMU7
	out		(c),a
	ld		bc,ZXN_REG_DATA
	in		a,(c)
	push	af

	call	Rem_GetRawByte	; Get Bank
	push	af				; save bank for later
	NextReg	ZXNR_MMU7,a
	call	Rem_GetRawWord	; Get offset
	ld		a,d
	or		$E0				; Force $E000+offset
	ld		h,a
	ld		l,e

	call	Rem_GetRawByte	; BP num
	add		a,a
	add		a,a
	ld		d,HIGH BPStore
	ld		e,a

	;bank and hl setup point to byte to set bp on
	;de points to our BP storage

	ld		a,(hl)			; Fetch opcode
	ld		(de),a			; Save to table
	inc		de
	pop		af
	ld		(de),a			; Save bank
	inc		de
	ld		a,l
	ld		(de),a			; save low offset
	inc		de
	ld		a,h
	ld		(de),a			; save high offset

	; At this point our breakpoint is saved away
	ld		(hl),$C7		; overwrite instruction with RST00

	; Restore MMU7
	pop		af
	NextReg	ZXNR_MMU7,a
	
	jp		Process

Execute:
	call	Rem_GetRawWord	; Address to jump to

	push	de
Resume:
	ret

GetState:

	ld		hl,(IFF2)
	push	hl
	ld		hl,(RegStoreIR)
	push	hl
	exx
	push	hl
	push	de
	push	bc
	exx
	ex		af,af'
	push	af
	ex		af,af'
	push	iy
	push	ix
	ld		hl,(RegStorePC)
	push	hl
	ld		hl,(RegStoreSP)
	push	hl
	ld		hl,(RegStoreHL)
	push	hl
	ld		hl,(RegStoreDE)
	push	hl
	ld		hl,(RegStoreBC)
	push	hl
	ld		hl,(RegStoreAF)
	push	hl

	ld		e,14
_GetStateLoop:
	pop		hl
	ld		a,l
	call	Rem_SendRawByte
	ld		a,h
	call	Rem_SendRawByte
	dec		e
	jr		nz,_GetStateLoop
	
	jp		Process

SetState:
	ld		l,14
_SetStateLoop:
	call	Rem_GetRawWord
	push	de
	dec		l
	jr		nz,_SetStateLoop

	pop		hl
	ld		(IFF2),hl
	pop		hl
	ld		(RegStoreIR),hl

	exx
	pop		hl
	pop		de
	pop		bc
	exx
	ex		af,af'
	pop		af
	ex		af,af'
	pop		iy
	pop		ix
	pop		hl
	ld		(RegStorePC),hl
	pop		hl
	ld		(RegStoreSP),hl
	pop		hl
	ld		(RegStoreHL),hl
	pop		hl
	ld		(RegStoreDE),hl
	pop		hl
	ld		(RegStoreBC),hl
	pop		hl
	ld		(RegStoreAF),hl

	jp		Process

SetIOPort:
	call	Rem_GetRawWord
	call	Rem_GetRawByte
	ld		c,e
	ld		b,d
	out		(c),a
	jp		Process

GetIOPort:
	call	Rem_GetRawWord
	ld		c,e
	ld		b,d
	in		a,(c)
	call	Rem_SendRawByte
	jp		Process


	db		"LOOKHERE"

; Currently we only back up what we use!

IFF2:
	ds		1
	ds		1

RegStoreIR:
	ds		2
RegStoreHL:
	ds		2
RegStoreDE:
	ds		2
RegStoreBC:
	ds		2
RegStoreAF:
	ds		2
RegStoreSP:
	ds		2
RegStorePC:
	ds		2

StackSpace:
	ds		20*2
StackPos:

	align	256
BPStore:
	ds		256					; TABLE IS 
								; byte originalOpcode
								; byte bank
								; word pc		(nb, this is stored as $E000 + offset)

FindBreakpoint:					; de contains offset | E000 , a contains bank num
	ld		b,a
	ld		hl,BPStore+1
_loop:
	ld		a,b
	cp		(hl)
	jr		nz,_bankMismatch

	inc		l
	ld		a,e
	cp		(hl)
	jr		nz,_lowOffsetMismatch

	inc		l
	ld		a,d
	cp		(hl)
	jr		nz,_hiOffsetMismatch

	;Found
	dec		l
	dec		l
	dec		l
	ret

_hiOffsetMismatch:
	dec		l
_lowOffsetMismatch:
	dec		l
_bankMismatch:
	ld		a,b
	add		a,4
	jr		nc,_loop

	; Failed to find breakpoint - this is all sorts of F00Bar
	jp		CRASH

CRASH:
	ld		a,0
_loop
	out		($FE),a
	inc		a
	jr		_loop


;; Will need logic to handle if we were executing code in slot 0 when Break hit - TODO

Breakpoint:						; TODO needs to grab registers, recover overwritten byte etc.
	ld		(RegStoreHL),hl
	ld		(RegStoreDE),de
	ld		(RegStoreBC),bc
	ld		(RegStoreSP),sp
	pop		hl
	dec		hl
	ld		(RegStorePC),hl

	ld		sp,StackPos			; setup our stack
	push	af
	pop		hl
	ld		(RegStoreAF),hl
	ld		a,i
	push	af
	ld		h,a
	ld		a,r
	ld		l,a
	ld		(RegStoreIR),hl
	pop		hl
	ld		a,l
	ld		(IFF2),a

	ld		hl,(RegStoreSP)
	ld		c,(hl)
	inc		hl
	ld		h,(hl)
	ld		l,c
	dec		hl

	push	hl
	; hl points to breakpoint - need to figure out bank and offset
	ld		e,l
	ld		a,h
	or		$E0
	ld		d,a					; correct offset to E000+offset style used for breakpoints

	ld		a,h					; bank num is BBBxxxxx
	swapnib
	srl		a
	and		$07
	or		$50					; now have bank number so we can fetch the bank
	
	ld		bc,ZXN_REG_NUM
	out		(c),a
	ld		bc,ZXN_REG_DATA
	in		a,(c)

	call	FindBreakpoint		; returns hl pointing to the BP slot

	ld		a,(hl)				; restore (restore original code from our temporary breakpoint (RST00)
	pop		hl
	ld		(hl),a

	call	Process

	ld		hl,(RegStoreIR)
	ld		a,l
	ld		i,a
	ld		a,h
	ld		r,a

	ld		hl,(RegStoreAF)
	push	hl
	pop		af
	ld		sp,(RegStoreSP)
	ld		bc,(RegStoreBC)
	ld		de,(RegStoreDE)
	ld		hl,(RegStorePC)
	push	hl
	push	af
	ld		hl,IFF2
	bit		2,(hl)
	jr		z,_enableInts

_leaveIntsDisabled:
	ld		hl,(RegStoreHL)
	pop		af
	ret
_enableInts:
	ld		hl,(RegStoreHL)
	pop		af
	ei
	ret

	include	"lib_remote.asm"

	end

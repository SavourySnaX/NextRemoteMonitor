; vim:  autoindent noexpandtab tabstop=4 shiftwidth=4
;
; Standalone Remote Control (Loads at 49152), used for testing/demonstration
; 


; Command Formats for Demonstration
;
;  Listed here in terms of Spectrum Next view of things (e.g. download means the next is recieving data)
;
;
;   Initial Byte    |   Description	        |   Params
;	---------------------------------------------------------------------
;       ??          | Exit                  | 	
;       01          | Download Data         |  BB   - Byte - Ignored
;                   |                       |  LLHH - Word - Destination Address
;                   |                       |  LLHH - Word - Length
;       02          | Upload Data           |  BB   - Byte - Ignored
;                   |                       |  LLHH - Word - Destination Address
;                   |                       |  LLHH - Word - Length
;
;

	include		"next.defs"

	org		$C000

	di
	ld		hl,ReadyMessage
	call	Rem_Initialise
	jp		Rem_Process

; Used to allow the SimpleHost to identify we are using the standalone remote version
ReadyMessage:	db		"RDY?",0


Rem_Process:

	; Here is where we process Commands (First byte in message is command)
_lp1:
	call	Rem_GetRawByte

	cp		2
	jr		z,_SendData
	cp		1
	jr		z,_RecvData

	; any other byte just exists this demonstration

_Exit:

	call	Rem_Close

	ei
	ret

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
	
	jr		_lp1

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

	jr		_lp1




	include	"lib_remote.asm"

	end		$C000

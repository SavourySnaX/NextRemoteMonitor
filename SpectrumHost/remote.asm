;
;               UART / WIFI Module experiments
;

DEBUG	equ	0		; switch to 1 to have commands/responses printed
				; to the spectrum screen while running

;
; My Spectrum Next Macros/Defines - TODO move to file
;

ZXN_REG_NUM     equ     $243B
ZXN_REG_DATA    equ     $253B

ZXN_UART_TX     equ     $133B
ZXN_UART_RX     equ     $143B

FUDGE_DELAY	equ	20


; used for printing when debugging
TVFLAG		equ	$5C3C


; Write to RX configures prescalar - see UART_Initialise

; Read of TX shows FIFO status
;       bit 0 - 0 if FIFO empty, 1 if data ready to be collected
;       bit 1 - 0 if TX idle, 1 if still transmitting last byte
;       bit 2 - 0 if buffer has space, 1 if full


        org     $C000

start:
	IF DEBUG
		xor	a
		ld	(TVFLAG),a
	ENDIF

        di

	; TODO - we should be able to boost beyond 115200 baud
        call    UART_Initialise

        call    UART_Clear	;; Flush all 512 bytes

        ld      hl,EchoOff
	call	TransmitAndDrain

	ld	hl,ConnectString
	call	TransmitAndDrain
	ld	hl,ContinousMode
	call	TransmitAndDrain
	ld	hl,BeginRaw
	call	TransmitAndDrain

	; At this point the wifi modem should be connected and send
	;recv will be raw bytes

	; send a quick test message
	ld	hl,ReadyMessage
	call	TransmitString

	; Here is where we process Commands (First byte in message is command
_lp1:
	call	GetRawByte

	cp	2
	jr	z,_SendData
	cp	1
	jr	z,_RecvData
	cp	0
	jr	nz,_lp1

_Exit:
        ; now we need to pause for a second, send the hang up, 
	;pause for another second and disconnect
	; It might be worth calling during startup to guarantee
	;the modem is reset (for instances where we crashed/didn't disconnect
	;properly.

	ld	a,60
	call	WaitForFrames

	ld	hl,HangUp
	call	TransmitString

	ld	a,60
	call	WaitForFrames

	ld	hl,Disconnect
	call	TransmitAndDrain

	ei
	ret

_RecvData:
	call	GetRawByte	; Get Bank - discard for now
	call	GetRawWord	; Get Addr
	ld	h,d
	ld	l,e
	call	GetRawWord
	ld	b,d
	ld	c,e

_RecvLoop:
	push	bc
	call	GetRawByte
	pop	bc
	ld	(hl),a
	inc	hl
	dec	bc
	ld	a,b
	or	c
	jr	nz,_RecvLoop
	
	jr	_lp1

_SendData:
	call	GetRawByte	; Get Bank - discard for now
	call	GetRawWord	; Get Addr
	ld	h,d
	ld	l,e
	call	GetRawWord
	ld	b,d
	ld	c,e

_SendLoop:
	push	bc
	ld	a,(hl)
	inc	hl
	call	SendRawByte
	pop	bc
	dec	bc
	ld	a,b
	or	c
	jr	nz,_SendLoop
	
	jr	_lp1



EchoOff: 		db      "ATE0",13,10,0
ConnectString:		db	"AT+CIPSTART=\"TCP\",\"192.168.5.2\",9999",13,10,0
ContinousMode:		db	"AT+CIPMODE=1",13,10,0
BeginRaw:		db	"AT+CIPSEND",13,10,0
HangUp:			db	"+++",0
Disconnect:		db	"AT+CIPCLOSE",13,10,0

; Not currently checked at all, remote proceeds once "anything is recieved"
ReadyMessage:		db	"RDY?",0



TransmitAndDrain:
	push	hl
	call	TransmitString
	ld	a,FUDGE_DELAY
	call	WaitForFrames
	call	DrainFIFO
	pop	hl
	call	PrintString
	ld 	hl,DrainBuffer
	jp	PrintString

WaitForFrames:
	ei
_lp1:
	halt
	dec	a
	jr	nz,_lp1
	di
	ret


PrintString
	
IF !DEBUG
	ret			; UNCOMMENT to enable printing
ELSE	

	ei

_lp1:
	ld	a,(hl)
	inc	hl

	and	a
	jr	z,_end

	cp	10
	jr	z,_lp1

	rst	$10

	jr	_lp1

_end:
	di
	ret
ENDIF

; UART MODULE CODE

; UART_Initialise
;-----
; IN
;-----
;-----
; OUT
;-----
; Corrupts hl,de,bc,a,f

UART_Initialise:
        ld      bc,ZXN_REG_NUM          ; 10
        ld      a,$11                   ; 7
        out     (c),a                   ; 12

        ld      bc,ZXN_REG_DATA         ; 10
        in      a,(c)                   ; 12

        and     7                       ; 7

; video mode in a (use as index lookup to set correct prescalar)

        ld      hl,UART_PRESCALAR       ; 10
        add     a,a                     ; 4
        add     a,l                     ; 4
        ld      l,a                     ; 4
        xor     a                       ; 4
        adc     a,h                     ; 4

        ld      e,(hl)                  ; 7
        inc     hl                      ; 6
        ld      d,(hl)                  ; 7

; prescalar adjustment in de    (note : its a 14bit value - written as 2 7 bit values, so needs slight adjustment to write correctly)

        ld      bc,ZXN_UART_RX          ; 10

        ld      a,e                     ; 4
        and     $7F                     ; 7
        out     (c),a                   ; 12

        rl      e                       ; 8
        rl      d                       ; 8
        ld      a,d                     ; 4
        or      $80                     ; 7
        out     (c),a                   ; 12

        ret				; 10



UART_PRESCALAR: dw      243,248,256,260,269,278,286,234

; UART_Clear
;
;
;-----
; IN
;-----
;-----
; OUT
;-----
;-----
; Corrupts bc,a,f

UART_Clear:
        ld      bc,ZXN_UART_RX          ; 10
        xor     a                       ; 4
@clear:
        in      d,(c)                   ; 12
        in      d,(c)                   ; 12
        dec     a                       ; 4
        jr      nz,@clear               ; 12
        ret                             ; 10

; TransmitString
;-----
; IN
;-----
; hl points to string to transmit
; (string is null terminated)
;-----
; OUT
;-----
; hl points at 0 terminator of string
;-----
; Corrupts bc,a,f

TransmitString:

        ld      bc,ZXN_UART_TX          ; 10

@wait1: ;; Wait for transmit ready
        in      a,(c)                   ; 12

        and     2                       ; 7
        jr      nz, @wait1              ; 12

        ld      a,(hl)                  ; 7
        and     a                       ; 4
        ret     z                       ; 11

        out     (c),a                   ; 12

        inc     hl                      ; 6

        jr      @wait1                  ; 12


; SendRawByte
;-----
; IN
;-----
;-----
; OUT
;-----
; a contains byte from FIFO
;-----
; Corrupts bc,a,f

SendRawByte:

	ld	bc, ZXN_UART_TX		; 10
	push	af			; 11
_lp:
	in	a,(c)			; 12

	and	2			; 7
	jr	nz,_lp			; 12

	pop	af			; 10
	out	(c),a			; 12
	ret				; 10


; GetRawByte
;-----
; IN
;-----
;-----
; OUT
;-----
; a contains byte from FIFO
;-----
; Corrupts bc,a,f

GetRawByte:

	ld	bc, ZXN_UART_TX		; 10
_lp:
	in	a,(c)			; 12

	and	1			; 7
	jr	z,_lp			; 12

	ld	bc, ZXN_UART_RX		; 10
	in	a,(c)			; 12
	ret				; 10


; GetRawWord
;-----
; IN
;-----
;-----
; OUT
;-----
; de contains word from FIFO
;-----
; Corrupts bc,a,f

GetRawWord:

	call	GetRawByte		; 10
	ld	e,a			; 4
	call	GetRawByte		; 10
	ld	d,a			; 4
	ret				; 10


; Note : DrainFIFO is currently used for debugging
; it will wrap at 256 characters


; DrainFIFO
;-----
; IN
;-----
;-----
; OUT
;-----
;-----
; Corrupts hl,bc,a,f

DrainFIFO:

        ;; Empty FIFO buffer to our internal drain buffer
        ld      hl,DrainBuffer          ; 10
        xor     a                       ; 4
        ld      (DrainCount),a          ; 13
	ld	(hl),a			; 7

@another:

        ld      bc,ZXN_UART_TX          ; 10
        in      a,(c)                   ; 12

        and     1                       ; 7
        ret     z                       ; 11

        ld      bc,ZXN_UART_RX          ; 10
        in      a,(c)                   ; 12

        ld      (hl),a                  ; 7
        inc     l                       ; 4

        ld      a,l                     ; 4
        ld      (DrainCount),a          ; 13

        jr      nz,@another             ; 12

        inc     a                       ; 4
        ld      (DrainCount+1),a        ; 13

        jr      @another                ; 12

DrainCount:
        dw      0

        align   256

DrainBuffer:
        ds      256


	end $C000

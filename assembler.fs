\ assembler.fs ---

\ Copyright 2011 (C) David Vazquez

\ This file is part of Eulex.

\ Eulex is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ Eulex is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with Eulex.  If not, see <http://www.gnu.org/licenses/>.

vocabulary Assembler
get-current
also Assembler definitions

\ Display the hexadecimal values temporarily
: emit-byte hex. ;
: emit-word hex. ;

\ Instructions with no operands
: single-instruction ( opcode -- )
    create c, does> c@ emit-byte ;

HEX
60 single-instruction pusha
61 single-instruction popa
90 single-instruction nop
C3 single-instruction ret
CF single-instruction iret
FA single-instruction cli
FB single-instruction sti
DECIMAL

01 constant OPREG8
02 constant OPREG16
04 constant OPREG32
08 constant OPSREG
16 constant OPIMM
32 constant OPMEM

\ Masks
OPREG8 OPREG16 or OPREG32 or constant OPGREG
OPGREG OPSREG  or constant OPREG

\ These words check for the type of the operand in the data
\ stack. They do _NOT_ consuming the operand, however.
: reg? over OPREG and 0<> ;
: reg32? over OPREG32 = ;
: reg16? over OPREG16 = ;
: reg8? over OPREG8 = ;
: sreg? over OPSREG = ;
: greg? over OPGREG and 0<> ;
: mem? over OPMEM = ;
: imm? over OPIMM = ;

\ Registers

: reg8  create , does> @  OPREG8 swap ;
: reg16 create , does> @ OPREG16 swap ;
: reg32 create , does> @ OPREG32 swap ;
: sreg  create , does> @  OPREGS swap ;

0 reg32 %eax     0 reg16 %ax     0 reg8 %al     0 sreg %es
1 reg32 %ecx     1 reg16 %cx     1 reg8 %cl     1 sreg %cs
2 reg32 %edx     2 reg16 %dx     2 reg8 %dl     2 sreg %ss
3 reg32 %ebx     3 reg16 %bx     3 reg8 %bl     3 sreg %ds
4 reg32 %esp     4 reg16 %sp     4 reg8 %ah     4 sreg %fs
5 reg32 %ebp     5 reg16 %bp     5 reg8 %ch     5 sreg %gs
6 reg32 %esi     6 reg16 %si     6 reg8 %dh
7 reg32 %edi     7 reg16 %di     7 reg8 %bh

\ Immediate values
: # OPIMM ;


\ Memory references

\ The more general memory reference mode is
\     base + index*scale + displacement
\ where BASE and INDEX are 32bits registers, SCALE is 1, 2 or 4, and
\ DISPLACEMENT is an immediate offset.
\
\ The following variables contain each one of the parts in the general
\ addressing mode. A value of -1 where a register is expected means
\ that it is omitted. Note that is it not the ModR/M either thea SIB
\ bytes. They are encoded later from this variables, however.
variable base
variable index
variable scale
variable displacement

: check-reg32
    reg32? invert abort" Addressing mode must use 32bits registers." ;

: B check-reg32 nip base ! ;
: I check-reg32 nip index ! ;
: S scale ! ;
: D displacement ! ;

: PTR D OPMEM 0 ;

: 1* 1 S ;
: 2* 2 S ;
: 4* 4 S ;

\ BASE                      BASE + DISP                   INDEX
: [%eax] %eax B * ;       : +[%eax] D [%eax] ;          : >%eax %eax I ;
: [%ecx] %ecx B * ;       : +[%ecx] D [%ecx] ;          : >%ecx %ecx I ;
: [%edx] %edx B * ;       : +[%edx] D [%edx] ;          : >%edx %edx I ;
: [%ebx] %ebx B * ;       : +[%ebx] D [%ebx] ;          : >%ebx %ebx I ;
: [%esp] %esp B * ;       : +[%esp] D [%esp] ;          : >%esp %esp I ;
: [%ebp] %ebp B * ;       : +[%ebp] D [%ebp] ;          : >%ebp %ebp I ;
: [%esi] %esi B * ;       : +[%esi] D [%esi] ;          : >%esi %esi I ;
: [%edi] %edi B * ;       : +[%edi] D [%edi] ;          : >%edi %edi I ;

\ Instructions

variable inst#op
variable instsize

: operands ( u -- )
    inst#op ! ;

: 32bits 32 instsize ! ;
: 16bits 16 instsize ! ;
:  8bits  8 instsize ! ;

\ Operands pattern maching
variable dispatchp

: 1-op-match ( op mask -- op flag )
    2 pick and 0<> ;

: 2-op-match ( op1 op2 mask1 mask2 -- op1 op2 flag )
    3 pick and 0<> swap
    5 pick and 0<> and ;

: (dispatch) ( ... matcher xt -- )
    >r execute r> swap if dispatchp on execute else drop then ;

: 1-dispatch ['] 1-op-match swap (dispatch) ;
: 2-dispatch ['] 2-op-match swap (dispatch) ;

: dispatcher
    dispatchp off ;

: dispatch
    dispatchp @ invert if
        inst#op @ 1 = if 1-dispatch else 2-dispatch endif
    endif ;

: end-dispatcher
    inst#op @ 2 = if 2drop endif 2drop
    dispatchp @ abort" Operand pattern did not match." ;


SET-CURRENT
( PREVIOUS )

\ assembler.fs ends here

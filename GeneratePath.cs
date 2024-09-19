using Godot;
using System;
using System.Numerics;
using System.Collections.Generic;

public partial class GeneratePath : Node
{
	public Dictionary<string, string[]> pathInfo = new() {
		{"ten", new string[]{"11","2102274"}},
		{"box", new string[]{"11","7345159"}},
		{"spaceChuckAtt", new string[]{"11","7345159"}},
		{"spaceChuckMov", new string[]{"11","2102274"}},
		{"spaceWallaceAtt", new string[]{"22","15411434243086"}},
		{"spaceWallaceMov", new string[]{"11","2102274"}},
		{"spaceCaesarAtt", new string[]{"33","32281873667108896796"}},
		{"spaceCaesarMov", new string[]{"11","2102274"}},
	};
	
	public BigInteger Path(string pathName, int position, 
		BigInteger selfboard, BigInteger enemyboard, bool isAttackMove, int expansionRate = 1)
	{
		GD.Print($"> Path : expansionRate[{expansionRate}]");
		
		BigInteger bitmap = 0;
		
		switch(pathName)
		{
			case "ten": bitmap = TenPath(position,selfboard,enemyboard,isAttackMove); break;
			case "box": bitmap = BoxPath(position,selfboard,enemyboard,isAttackMove); break;
			case "spaceChuckAtt": bitmap = GetGeneratePath(pathName,position,selfboard,enemyboard,isAttackMove); break;
			case "spaceChuckMov": bitmap = GetGeneratePath(pathName,position,selfboard,enemyboard,isAttackMove); break;
			case "spaceWallaceAtt": bitmap = GetGeneratePath(pathName,position,selfboard,enemyboard,isAttackMove); break;
			case "spaceWallaceMov": bitmap = GetGeneratePath(pathName,position,selfboard,enemyboard,isAttackMove); break;
			case "spaceCaesarAtt": bitmap = GetGeneratePath(pathName,position,selfboard,enemyboard,isAttackMove); break;
			case "spaceCaesarMov": bitmap = GetGeneratePath(pathName,position,selfboard,enemyboard,isAttackMove); break;
			default: return 0;
		}
		
		//기본 배율(1.0)에서 높을 경우
		//기본범위에서 확대한 범위를 추가함
		bitmap = Amplifier(position, bitmap, expansionRate);
		
		//이동
		//이동할 위치에 플레이어나 장애물이 있을 경우 배제
		if(pathName=="spaceChuckMov"
			|| pathName=="spaceWallaceMov" 
			|| pathName=="spaceCaesarMov")
		{
			//bitmap &= ~selfboard;
			//bitmap &= ~enemyboard;
		}
		//범위 내의 적이 위치하는 경우만 표시
		else if(pathName=="spaceChuckAtt"
			|| pathName=="spaceWallaceAtt"
			|| pathName=="spaceCaesarAtt")
		{
			//bitmap &= enemyboard;
		}
		
		return bitmap;
	}
	
	public BigInteger GetGeneratePath(string pathName, int position, 
		BigInteger selfboard, BigInteger enemyboard, bool isAttackMove)
	{
		BigInteger legalPath = BigInteger.Parse(pathInfo[pathName][1]);
		int pathCenterPoint = Convert.ToInt32(pathInfo[pathName][0]);
		BigInteger bitmap = 0;
		bitmap = TenPath2(legalPath, pathCenterPoint, position, selfboard, enemyboard, isAttackMove);

		return bitmap;
	}
	
	//Main의 버튼으로 Path테스트
	public void BitTest(int slotNumber, int stepUp)
	{
		string pathName = "";
		switch(slotNumber)
		{
			case 0: pathName = "spaceChuckAtt"; break;
			case 1: pathName = "spaceChuckMov"; break;
			case 2: pathName = "spaceWallaceAtt"; break;
			case 3: pathName = "spaceWallaceMov"; break;
			case 4: pathName = "spaceCaesarAtt"; break;
			case 5: pathName = "spaceCaesarMov"; break;
			case 6: pathName = "spaceChuckAtt"; break;
		}
		
		slotNumber = Convert.ToInt32(pathInfo[pathName][0]);
		
		System.Numerics.BigInteger blackBoard = 0;
		System.Numerics.BigInteger whiteBoard = 0;
		System.Numerics.BigInteger bitmap = new GeneratePath().Path(
			pathName, slotNumber, blackBoard, whiteBoard, false);
		BigInteger ampBit = Amplifier(slotNumber, bitmap, stepUp);
		
		BitPrint(bitmap);
		BitPrint(ampBit);
	}
	
	public void BitPrint(BigInteger bitmap)
	{
		int interval = 10;
		string strBits = Bitboard.BigIntegerToBinaryString(bitmap); //bin->str
		strBits = strBits.PadLeft(100,'0');
		int cnt = (int)Math.Ceiling((double)(strBits.Length / interval));
		string result = "";
		
		for(int i=0; i<cnt; i++)
		{
			result += strBits.Substring(i*interval, interval);
			if(i+1 < cnt) result += "\r\n";
		}
		
		result = $"[{bitmap}]\r\n{result}\r\n";
		result += $"---------------------------------------------";
		GD.Print(result);
	}
	
	//Path Extender
	public BigInteger Amplifier(int center, BigInteger bitmap, int scale)
	{
		int xsize = 10;
		int ysize = 10;
		int yweight = ysize;
		string str = Bitboard.BigIntegerToBinaryString(bitmap);//objSlot.Text;
		str = str.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
		int cp = center;//centerPoint;
		
		char[] chArrReverse = str.ToCharArray();
		Array.Reverse(chArrReverse);
		BigInteger exp = 0;
		int cx = cp%xsize;
		int cy = (cp-cx)/ysize;
		BigInteger ep = 0;//적용위치
		
		GD.Print($"> cp[{cp}] cx[{cx}] cy[{cy}]");
		
		for(int i=0; i<chArrReverse.Length; i++)
		{
			if(chArrReverse[i]=='1')
			{
				//현재 좌표 위치
				int x = i%10;
				int y = (i-x)/ysize;
				//center point와 현재 좌표간 간격
				int gx = Math.Abs(cx-x);
				int gy = Math.Abs(cy-y);
				//center point에서 확장된 좌표 위치
				int ex = 0;
				int ey = 0;
				
				if((cp-(gx*scale) >= 0 && cp+(gx*scale) <= 99) //확장 좌표의 하단
					&& ((i < cp && (cp-(gy*scale*yweight) >= 0) 
						|| (i > cp && cp+(gy*scale*yweight) <= 99)))) //확장 좌표의 상단
				{
					if(x < cx && Math.Abs((x-gx)%10) < cx && x-gx >= 0)		//cx기준 우측확장
					{
						ex = cx - (gx * scale);
					}
					else if(x > cx && (x+gx)%10 > cx)					//cx기준 좌측확장
					{
						ex = cx + (gx * scale);
					}
					else if((x-gx) >= 0 && (x+gx) <= 9 )
					{
						ex = cx;
					}
					
					if(y < cy && Math.Abs((y-gy)%10) < cy)			//cy기준 하단확장
					{
						ey = cy - (gy * scale);
					}
					else if(y > cy && Math.Abs((y+gy)%10) > cy)	//cy기준 상단확장
					{
						ey = cy + (gy * scale);
					}
					else if ((y-gy) >= 0 && (y+gy) <= 9)
					{
						ey = cy;
					}
					
					if(cp != (ey*yweight) + ex
						&& ((x-gx) >= 0 && (x+gx) <= 9)
						&& ((y-gy) >= 0 && (y+gy) <= 9))
						ep += new BigInteger(Math.Pow(2,(ey*yweight) + ex));
					
					GD.Print($"> i[{i}] cx,cy[{cx},{cy}] gx,gy[{gx},{gy}]");
					GD.Print($"> [{cp-(gx*scale) >= 0 && cp+(gx*scale) <= 99}, {cp-(gx*scale)} >= 0 && {cp+(gx*scale)} <= 99]  cp-(gx*scale) >= 0 && cp+(gx*scale) <= 99");
					GD.Print($"> [{cp-(gy*scale*yweight) >= 0 && cp+(gy*scale*yweight) <= 99}, {cp-(gy*scale*yweight)} >= 0 && {cp+(gy*scale*yweight)} <= 99]  cp-(gy*scale*yweight) >= 0 && cp+(gy*scale*yweight) <= 99");
					GD.Print($"> x,y[{x},{y}] gx,gy[{gx},{gy}] ex,ey[{ex},{ey}] ep[{ep}]");
				}
			}
		}
		
		exp = ep;
		BigInteger strDec = Bitboard.BinaryStringToBigInteger(str);
		string sumBits = Bitboard.BigIntegerToBinaryString(strDec | exp);
		
		GD.Print($"> str\r\n{str}");
		GD.Print($"> exp\r\n{Bitboard.BigIntegerToBinaryString(exp)}");
		GD.Print($"> sumBits\r\n{sumBits}");
		
		return Bitboard.BinaryStringToBigInteger(sumBits);
	}
}

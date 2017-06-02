#define FWD_L_PWM 3
#define BACK_L_PWM 4
#define FWD_R_PWM 23
#define BACK_R_PWM 22
#define BRAKES 21
#define Ultra_en 20
#define trig_r 1
#define echo_r 2
#define trig_mr 5
#define echo_mr 6
#define trig_m 7
#define echo_m 8
#define trig_ml 9
#define echo_ml 10
#define trig_l 11
#define echo_l 12

void setup() {
	
	// open the serial port
	Serial.begin(9600);
 
	// Motion control
	pinMode(FWD_L_PWM, OUTPUT);
	pinMode(BACK_L_PWM, OUTPUT);
	pinMode(FWD_R_PWM, OUTPUT);
	pinMode(BACK_R_PWM, OUTPUT);
	pinMode(BRAKES, OUTPUT);
	
	// Ulatrasound sensors
	pinMode(Ultra_en, OUTPUT); 
	pinMode(trig_l, OUTPUT);
	pinMode(echo_l, INPUT);
	pinMode(trig_ml, OUTPUT);
	pinMode(echo_ml, INPUT);
	pinMode(trig_m, OUTPUT);
	pinMode(echo_m, INPUT);
	pinMode(trig_mr, OUTPUT);
	pinMode(echo_mr, INPUT);
	pinMode(trig_r, OUTPUT);
	pinMode(echo_r, INPUT);	
	delay(1000);
	
	// Init all pins
	analogWrite(FWD_L_PWM, 0);
	analogWrite(BACK_L_PWM, 0);
	analogWrite(FWD_R_PWM, 0);
	analogWrite(BACK_R_PWM, 0);
	digitalWrite(BRAKES, HIGH);
	digitalWrite(Ultra_en, HIGH);
	digitalWrite(trig_l, LOW);
	digitalWrite(trig_ml, LOW);
	digitalWrite(trig_m, LOW);
	digitalWrite(trig_mr, LOW);
	digitalWrite(trig_r, LOW);
	delay(1000);
}

int len = 12;
char cmd[12] = {'0', '0', '0', '0', '1', '0', '0', '0', '1', '0', '0', '0'};
int leftSpeed = 0;
int rightSpeed = 0;
int distance_l = 0; // in centimeters
int distance_ml = 0;
int distance_m = 0;
int distance_mr = 0;
int distance_r = 0;
String lstate="1";
String rstate="1";

void Left() {
	analogWrite(FWD_L_PWM, 0);
	analogWrite(BACK_L_PWM, 0);
	analogWrite(FWD_R_PWM, 30);
	analogWrite(BACK_R_PWM, 0);
	digitalWrite(BRAKES, HIGH);
	lstate="1";
	rstate="2";
	leftSpeed=0;
	rightSpeed=30;
}

void Right() {
	analogWrite(FWD_L_PWM, 30);
	analogWrite(BACK_L_PWM, 0);
	analogWrite(FWD_R_PWM, 0);
	analogWrite(BACK_R_PWM, 0);
	digitalWrite(BRAKES, HIGH);
	lstate="2";
	rstate="1";
	leftSpeed=30;
	rightSpeed=0;
}

void Stall() {
	analogWrite(FWD_L_PWM, 0);
	analogWrite(BACK_L_PWM, 0);
	analogWrite(FWD_R_PWM, 0);
	analogWrite(BACK_R_PWM, 0);
	digitalWrite(BRAKES, LOW);
	lstate="1";
	rstate="1";
	leftSpeed=0;
	rightSpeed=0;
}

int charToInt(char ch)
{
	int tmp = 0;
	tmp = tmp * 10 + (ch - 48);
	return tmp;
}

String intTostring(int n)
{
	int n1 = n%10;
	String s1=String(n1);
	n=n/10;
	int n2 = n%10;
	String s2=String(n2);
	int n3=n/10;
	String s3=String(n3);
	return s3+s2+s1;
}

// state -> 0: fwd, back, Stall; <0: clockwise/right turn; >0: clounterclockwise/left turn;
bool Reactive(int r, int l)
{
	rstate = String(r);
	lstate = String(l);
	int state=r-l;
	
	digitalWrite(trig_l, LOW);  
	delayMicroseconds(2); 
	digitalWrite(trig_l, HIGH);
	delayMicroseconds(10); 
	digitalWrite(trig_l, LOW);  
	int duration_l = pulseIn(echo_l, HIGH);
  
	digitalWrite(trig_ml, LOW);  
	delayMicroseconds(2); 
	digitalWrite(trig_ml, HIGH);
	delayMicroseconds(10); 
	digitalWrite(trig_ml, LOW);  
	int duration_ml = pulseIn(echo_ml, HIGH);

	digitalWrite(trig_m, LOW);  
	delayMicroseconds(2); 
	digitalWrite(trig_m, HIGH);
	delayMicroseconds(10); 
	digitalWrite(trig_m, LOW);  
	int duration_m = pulseIn(echo_m, HIGH);

	digitalWrite(trig_mr, LOW);  
	delayMicroseconds(2); 
	digitalWrite(trig_mr, HIGH);
	delayMicroseconds(10); 
	digitalWrite(trig_mr, LOW);  
	int duration_mr = pulseIn(echo_mr, HIGH);

	digitalWrite(trig_r, LOW);  
	delayMicroseconds(2); 
	digitalWrite(trig_r, HIGH);
	delayMicroseconds(10); 
	digitalWrite(trig_r, LOW);  
	int duration_r = pulseIn(echo_r, HIGH);
  
	distance_l = duration_l/58; // in centimeters
	distance_ml = duration_ml/58;
	distance_m = duration_m/58;
	distance_mr = duration_mr/58;
	distance_r = duration_r/58;
	
	if ( (r==2) && (l==2) ) {
		if ( (distance_m < 80) || ( (distance_ml < 80)&&(distance_mr < 80) ) ) {
			Stall();
			return false;
		}
		else {
			if ( (distance_ml < 80) || (distance_l < 20) ) {
				Right();
				return false;
			}
			if (distance_mr < 80 || (distance_r < 20) ) {
				Left();
				return false;
			}
		}
	}
	// Clockwise/Right turn
	if (state<0) {
		if ( (distance_r < 20) || (distance_ml <0) )
		{
			Stall();
			return false;
		}
	}
	// Clounterclockwise/Left turn
	if (state>0) {
		if ( (distance_l < 20) || (distance_mr < 20) )
		{
			Stall();
			return false;
		}
	}
	return true;
}

void DataPrint() {
	Serial.println("DATA"+lstate+intTostring(leftSpeed)+rstate+intTostring(rightSpeed)+intTostring(distance_l)+intTostring(distance_ml)+intTostring(distance_m)+intTostring(distance_mr)+intTostring(distance_r) );
	delay(10);
}

void loop() {
	if (Serial.available() > 0) {
		Serial.readBytes(cmd, len);
	}
	
	// Analyse command
	if (cmd[0] == 'D' && cmd[1] == 'K') {

		// set left wheel direction and speed
		leftSpeed = charToInt(cmd[5]) * 100 + charToInt(cmd[6]) * 10 + charToInt(cmd[7]);
		if (leftSpeed>255) {
		  leftSpeed=0;
		}
	
		// set right wheel direction and speed
		rightSpeed = charToInt(cmd[9]) * 100 + charToInt(cmd[10]) * 10 + charToInt(cmd[11]);
		if (rightSpeed>255) {
		  rightSpeed=0;
		}
		
		// whether the Brake is enabled
		switch (cmd[3]) {
			case '0':
				digitalWrite(BRAKES, HIGH);
				break;
			default:
				digitalWrite(BRAKES, LOW);
				Stall();
				break;
		}
		
		switch (cmd[4]) {
			// Backward
			case '0':
				analogWrite(FWD_L_PWM, 0);
				analogWrite(BACK_L_PWM, leftSpeed);
				break;
			// Forward
			case '2':			
				analogWrite(FWD_L_PWM, leftSpeed);
				analogWrite(BACK_L_PWM, 0);
				break;   
			// Stall
			default:
				analogWrite(FWD_L_PWM, 0);
				analogWrite(BACK_L_PWM, 0);
				break;
		}

		switch (cmd[8]) {
			// Backward
			case '0':
				analogWrite(FWD_R_PWM, 0);
				analogWrite(BACK_R_PWM, rightSpeed);
				break;
			// Forward
			case '2':
				analogWrite(FWD_R_PWM, rightSpeed);
				analogWrite(BACK_R_PWM, 0);
				break;
			// Stall
			default:
				analogWrite(FWD_R_PWM, 0);
				analogWrite(BACK_R_PWM, 0);
				break;
		}	
		
		// Reactive control
		if (cmd[2] == '1') {
			// Enable all ultrasound sensors
			digitalWrite(Ultra_en, LOW);
			while(!Reactive(charToInt(cmd[8]), charToInt(cmd[4]))) {
				DataPrint();
				if (Serial.available() > 0) {
					break;
				}
			}
		DataPrint();
		}			
		else {
			digitalWrite(Ultra_en, HIGH);
		}
		
	}
	// Error command
	else {
		Serial.flush();
		Stall();		
	}
}

uniform sampler2D space;
uniform sampler2D disc;
uniform vec2 resolution;
uniform float iGlobalTime;
uniform vec2 center;
const float radius = 0.04;
const float pi = 3.1415927;
float sdSphere( vec3 p, float s )
{
  return length(p)-s;
}

float sdCappedCylinder( vec3 p, vec2 h )
{
  vec2 d = abs(vec2(length(p.xz),p.y)) - h;
  return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float sdTorus( vec3 p, vec2 t )
{
  vec2 q = vec2(length(p.xz)-t.x,p.y);
  return length(q)-t.y;
}

void main() {
	vec2 pp = gl_FragCoord.xy/resolution.xy;
	pp = -1.0 + 2.0*pp;
	pp.x *= resolution.x/resolution.y;
	pp -= center;
	pp *= 2.0;

	vec3 lookAt = vec3(0.0, -0.1, 0.0);
	
	// float eyer = 2.0;
	// float eyea = (iMouse.x / resolution.x) * pi * 2.0;
	// float eyea2 = ((iMouse.y / resolution.y)-0.24) * pi * 2.0;
	
	// vec3 ro = vec3(
	// 	eyer * cos(eyea) * sin(eyea2),
	// 	eyer * cos(eyea2),
	// 	eyer * sin(eyea) * sin(eyea2)); //camera position
	
	vec3 ro = vec3(0.0, 0.3, 2.0);
	
	vec3 front = normalize(lookAt - ro);
	vec3 left = normalize(cross(normalize(vec3(0.0,1,-0.1)), front));
	vec3 up = normalize(cross(front, left));
	vec3 rd = normalize(front*1.5 - left*pp.x + up*pp.y); // rect vector
	
	
	vec3 bh = vec3(0.0,0.0,0.0);
	// vec3 bh = vec3(center * 2.0, 0.0);
	float bhr = 0.05;
	float bhmass = 5.0;
	bhmass *= 0.001; // premul G
	
	vec3 p = ro;
	vec3 pv = rd;
	float dt = 0.02;
	
	vec3 col = vec3(0.0);
	
	float noncaptured = 1.0;
	
	vec3 c1 = vec3(0.5,0.46,0.4);
	vec3 c2 = vec3(1.0,0.8,0.6);
	
	// loop statement can only use int	
	for (int i=0; i<200; i+=1)
	{
		
		p += pv * dt * noncaptured;
		
		// gravity
		vec3 bhv = bh - p;
		float r = dot(bhv,bhv);
		pv += normalize(bhv) * ((bhmass) / r);
		
		noncaptured = smoothstep(0.0, 0.666, sdSphere(p-bh,bhr));
		
		// Texture for the accretion disc
		float dr = length(bhv.xz);
		float da = atan(bhv.z,bhv.x);
		vec2 ra = vec2(dr,da * (0.01 + (dr - bhr)*0.002) + 2.0 * pi + iGlobalTime*0.00001 );
		ra *= vec2(10.0,20.0);
		
		vec3 dcol = mix(c2,c1,pow(length(bhv)-bhr,2.0)) * max(0.0,texture2D(disc,ra*vec2(0.1,0.5)).r+0.05) * (4.0 / ((0.001+(length(bhv) - bhr)*50.0) ));
		
		col += max(vec3(0.0),dcol * smoothstep(0.0, 1.0, -sdTorus( ((p-bh) * vec3(1.0,25.0,1.0)), vec2(0.8,0.99))) * noncaptured);
		
		//col += dcol * (1.0/dr) * noncaptured * 0.001;
		
		// Glow
		col += vec3(1.0,0.9,0.85) * (1.0/vec3(dot(bhv,bhv))) * 0.0033 * noncaptured;
	}
	
	// BG
	//col += pow(textureCube(space, pv).rgb,vec3(3.0)) * noncaptured;
	vec3 dir = vec3(0.0);
	dir.x = dot(pv, -left);
	dir.y = dot(pv, up);
	dir.z = dot(pv, front);
	if (dir.z > 0.0) {
		dir.xy *= 1.5 / dir.z;
		dir.xy /= 2.0;
		dir.xy += center;
		dir.xy = dir.xy * 0.5 + 0.5;
		col += pow(texture2D(space, dir.xy).rgb, vec3(1.5)) * noncaptured;
	}

	// FInal color
	gl_FragColor = vec4(col,1.0);
}

// Original, dependency-free 3D geometry study. Embedded in loading-3d-theme-studies.html.
// Mesh vertices are transformed in 3D, lit, perspective-projected and depth sorted.
function createLoadingScene(canvas, theme) {
  const ctx = canvas.getContext('2d');
  const TAU = Math.PI * 2, pieces = [];
  let time = 0, previous = 0, frame = 0, running = false, disposed = false;
  let width = 480, height = 350;
  const material = (color, metal = 0) => ({ color: color.match(/\w\w/g).map(x => parseInt(x, 16)), metal });
  const silver = material('a9c5d4', .9), mint = material('a1c9bd', .65), blue = material('537cae', .8);
  const graphite = material('384854', .35), gold = material('b89751', .8), chalk = material('ada58d', .12);
  const ink = material('303830'), violet = material('756c9a', .6), cyan = material('87c9d8', .65);
  const rotate = (p, r) => {
    let [x,y,z] = p, c = Math.cos(r[0]), s = Math.sin(r[0]);
    [y,z] = [y*c-z*s,y*s+z*c]; c=Math.cos(r[1]);s=Math.sin(r[1]);
    [x,z] = [x*c+z*s,-x*s+z*c];c=Math.cos(r[2]);s=Math.sin(r[2]);
    return [x*c-y*s,x*s+y*c,z];
  };
  function add(mesh, mat, position=[0,0,0], rotation=[0,0,0], scale=[1,1,1], motion=null) {
    pieces.push({mesh,mat,position,rotation,scale,motion});
  }
  function ring(radius, tube, arc=TAU, segments=72, sides=16) {
    const vertices=[], faces=[];
    for(let i=0;i<=segments;i++)for(let j=0;j<sides;j++){
      const u=i/segments*arc,v=j/sides*TAU;
      vertices.push([(radius+tube*Math.cos(v))*Math.cos(u),(radius+tube*Math.cos(v))*Math.sin(u),tube*Math.sin(v)]);
    }
    for(let i=0;i<segments;i++)for(let j=0;j<sides;j++){
      const a=i*sides+j,b=(i+1)*sides+j,c=(i+1)*sides+(j+1)%sides,d=i*sides+(j+1)%sides;
      faces.push([a,b,c,d]);
    }
    if(arc<TAU){faces.push(Array.from({length:sides},(_,j)=>sides-1-j));faces.push(Array.from({length:sides},(_,j)=>segments*sides+j));}
    return {vertices,faces};
  }
  function sphere(radius, segments=40, rows=20) {
    const vertices=[],faces=[];
    for(let i=0;i<=rows;i++)for(let j=0;j<segments;j++){
      const v=i/rows*Math.PI,u=j/segments*TAU;
      vertices.push([radius*Math.sin(v)*Math.cos(u),radius*Math.cos(v),radius*Math.sin(v)*Math.sin(u)]);
    }
    for(let i=0;i<rows;i++)for(let j=0;j<segments;j++){
      const a=i*segments+j,b=i*segments+(j+1)%segments,c=(i+1)*segments+(j+1)%segments,d=(i+1)*segments+j;
      faces.push(i===0?[a,c,d]:i===rows-1?[a,b,c]:[a,b,c,d]);
    }
    return {vertices,faces};
  }
  function prism(points,depth) {
    const vertices=points.map(([x,y])=>[x,y,-depth/2]).concat(points.map(([x,y])=>[x,y,depth/2]));
    const n=points.length,faces=[Array.from({length:n},(_,i)=>n-1-i),Array.from({length:n},(_,i)=>n+i)];
    for(let i=0;i<n;i++)faces.push([i,(i+1)%n,(i+1)%n+n,i+n]);
    return {vertices,faces};
  }
  const box=prism([[-.5,-.5],[.5,-.5],[.5,.5],[-.5,.5]],1);
  const crystal={vertices:[[0,1.5,0],[.62,0,0],[0,0,.62],[-.62,0,0],[0,0,-.62],[0,-1.3,0]],faces:[[0,2,1],[0,3,2],[0,4,3],[0,1,4],[5,1,2],[5,2,3],[5,3,4],[5,4,1]]};
  const spin = speed => t=>({rotation:[0,t*speed,0]});
  const float = (speed=1,amount=.1)=>t=>({position:[0,Math.sin(t*speed)*amount,0]});
  const diamond=prism([[0,1],[1,0],[0,-1],[-1,0]],.18);
  if(theme==='neumorphic') {
    add(ring(1.13,.22),graphite,[0,0,0]);
    add(ring(.9,.035,TAU*.76),silver,[0,0,.24],[0,0,0],[1,1,1],t=>({rotation:[0,0,-t*.85]}));
    add(sphere(.7),graphite,[0,0,-.08],[0,0,0],[1,1,.4]);
    add(sphere(.17),cyan,[0,0,.37]);
  } else if(theme==='soft') {
    add(ring(1.08,.115),silver,[0,0,0],[.72,.25,-.5],[1,1,1],t=>({rotation:[0,t*.34,0]}));
    add(ring(1.03,.09),mint,[0,0,0],[-.65,.6,.55],[1,1,1],t=>({rotation:[0,-t*.28,0]}));
    add(sphere(.39),blue,[0,0,0],[0,0,0],[1,1,1],float(.9,.11));
  } else if(theme==='night') {
    add(ring(1.1,.065,TAU*.86),silver,[0,0,0],[.4,.25,0],[1,1,1],t=>({rotation:[0,0,t*.3]}));
    add(ring(.88,.12),violet,[0,0,-.04],[.4,.25,0]);
    add(sphere(.8),violet,[0,0,.02],[.4,.25,0],[1,1,.2],t=>({rotation:[Math.sin(t*.55)*.18,Math.sin(t*.35)*.3,0]}));
  } else if(theme==='ink') {
    add(ring(1.02,.12,TAU*.95,48,8),ink,[0,0,0],[0,0,0],[1,1,1],t=>({rotation:[0,0,t*.22]}));
    add(diamond,ink,[0,0,.04],[0,0,0],[.6,.6,1],spin(.32));
    add(box,material('d9d4c2'),[0,0,.19],[0,0,0],[.22,.22,.24]);
  } else if(theme==='fantasy') {
    add(ring(1.15,.055),gold);
    add(ring(1.02,.025),gold,[0,0,-.12]);
    add(diamond,gold,[0,0,0],[0,0,0],[.93,.93,1],spin(.24));
    add(diamond,material('542b28',.4),[0,0,.14],[0,0,0],[.7,.7,1],spin(.24));
    add(crystal,gold,[0,0,.25],[0,0,0],[.42,.43,.42],spin(.44));
    add(sphere(.1,8,4),gold,[0,1.26,0]);add(sphere(.1,8,4),gold,[0,-1.26,0]);
  } else if(theme==='jrpg') {
    add(crystal,material('93c7e9',.55),[0,0,0],[0,.25,.07],[.88,.85,.88],t=>({rotation:[0,t*.6,0],position:[0,Math.sin(t)*.08,0]}));
    add(crystal,silver,[-.95,-.5,0],[.2,.3,-.3],[.24,.24,.24],spin(-.8));
    add(crystal,silver,[.92,.7,-.2],[0,0,.4],[.17,.17,.17],spin(.8));
  } else if(theme==='pixel') {
    const blocks=[[-1,0],[0,-1],[0,0],[0,1],[1,0]];
    for(const [x,y] of blocks)add(box,x===0&&y===0?gold:violet,[x*.55,y*.55,0],[0,0,0],[.53,.53,.53]);
    add(box,material('bfb4d9'),[-1.15,.95,0],[0,0,0],[.26,.26,.26],t=>({position:[0,Math.floor((Math.sin(t*1.2)+1)*2)*.05,0]}));
    add(box,material('bfb4d9'),[1.15,-.95,0],[0,0,0],[.26,.26,.26]);
  } else if(theme==='scifi') {
    add(ring(1.24,.025),silver,[0,0,0],[.45,.1,0]);
    add(ring(1.07,.045),cyan,[0,0,0],[1.15,.6,.25],[1,1,1],t=>({rotation:[0,t*.35,0]}));
    add(ring(1.07,.035),blue,[0,0,0],[.1,1.3,-.4],[1,1,1],t=>({rotation:[t*.3,0,0]}));
    add(sphere(.4,20,10),silver,[0,0,0]);
    add(sphere(.1,12,6),cyan,[0,0,0],[0,0,0],[1,1,1],t=>({position:[Math.cos(t*.8)*1.24,Math.sin(t*.8)*1.24,0]}));
  } else if(theme==='military') {
    const chevron=prism([[-.88,.3],[0,-.08],[.88,.3],[.88,.04],[0,-.36],[-.88,.04]],.2);
    add(chevron,material('9cb7ae',.5),[0,.38,0],[0,0,0],[1,1,1],float(1.4,.045));
    add(chevron,material('588f89',.45),[0,-.26,0],[0,0,0],[1,1,1],float(1.4,.045));
    for(const x of [-1.13,1.13])for(const y of [-1.03,1.03]){
      add(box,silver,[x,y,-.13],[0,0,0],[.35,.035,.09]);
      add(box,silver,[x+(x<0?-.16:.16),y+(y<0?.16:-.16),-.13],[0,0,0],[.035,.35,.09]);
    }
    for(let i=0;i<9;i++)add(box,graphite,[(i-4)*.17,-.9,0],[0,0,0],[.055,.13,.1]);
  } else {
    add(ring(1.05,.055,TAU*.72,56,7),chalk,[0,0,-.1],[0,.15,.5]);
    add(ring(.83,.028,TAU*.68,48,6),material('6c7766'),[0,0,.1],[0,-.15,-1.2]);
    add(prism([[-.13,-1.2],[.04,-1.15],[.2,1.2],[.03,1.13]],.2),chalk,[0,0,.13],[0,0,-.1], [1,1,1],t=>({rotation:[0,Math.sin(t*.6)*.2,0]}));
    add(box,material('665044'),[0,-.3,.02],[0,0,.22],[1.7,.035,.06]);
  }
  function draw() {
    const ratio=Math.min(window.devicePixelRatio||1,2);
    width=canvas.clientWidth||480;height=canvas.clientHeight||350;
    if(canvas.width!==Math.round(width*ratio)||canvas.height!==Math.round(height*ratio)){canvas.width=Math.round(width*ratio);canvas.height=Math.round(height*ratio);}
    ctx.setTransform(ratio,0,0,ratio,0,0);ctx.clearRect(0,0,width,height);
    const unit=Math.min(width*.3,height*.34),faces=[];
    let view=[.24,Math.sin(time*.3)*.2-.25,-.08];
    if(theme==='pixel')view=[.4,Math.floor(time*1.8)*.16+.45,-.1];
    if(theme==='jrpg')view=[.08,-.2,0];
    if(theme==='military')view=[.12,Math.sin(time*.48)*.42-.3,0];
    if(theme==='horror')view=[.1,Math.sin(time*.28)*.24-.2,-.08];
    const project=p=>[width/2+p[0]*unit*5.6/(5.6-p[2]),height*.46-p[1]*unit*5.6/(5.6-p[2])];
    const shadow=ctx.createRadialGradient(width/2,height*.88,1,width/2,height*.88,unit*1.15);
    shadow.addColorStop(0,theme==='soft'||theme==='ink'?'#23303a26':'#00000065');shadow.addColorStop(1,'#00000000');
    ctx.save();ctx.translate(0,height*.88);ctx.scale(1,.16);ctx.translate(0,-height*.88);ctx.fillStyle=shadow;ctx.fillRect(0,0,width,height*2);ctx.restore();
    for(const part of pieces){
      const motion=part.motion?part.motion(time):{},r=part.rotation.map((v,i)=>v+(motion.rotation?.[i]||0)),pos=part.position.map((v,i)=>v+(motion.position?.[i]||0));
      const vertices=part.mesh.vertices.map(v=>rotate(rotate(v.map((a,i)=>a*part.scale[i]),r).map((a,i)=>a+pos[i]),view));
      for(const indices of part.mesh.faces){
        const points=indices.map(i=>vertices[i]),a=points[0],b=points[1],c=points[2];
        const u=b.map((v,i)=>v-a[i]),v=c.map((n,i)=>n-a[i]);
        let normal=[u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0]];
        const length=Math.hypot(...normal);if(length<.00001)continue;normal=normal.map(n=>n/length);
        // Double-sided lighting keeps thin crests and inner ring surfaces visible.
        if(normal[2]<0)normal=normal.map(n=>-n);
        const diffuse=Math.max(0,normal[0]*-.4+normal[1]*.65+normal[2]*.64);
        const spec=Math.pow(Math.max(0,normal[0]*-.28+normal[1]*.43+normal[2]*.86),24)*part.mat.metal;
        const strip=Math.pow(Math.max(0,1-Math.abs(normal[1]-.48)*4),7)*part.mat.metal*.25;
        const color=part.mat.color.map(n=>Math.round(Math.min(255,n*(.32+diffuse*.69)+spec*105+strip*75)));
        faces.push({points:points.map(project),depth:points.reduce((n,p)=>n+p[2],0)/points.length,color:'rgb('+color.join(',')+')'});
      }
    }
    faces.sort((a,b)=>a.depth-b.depth);
    for(const face of faces){ctx.beginPath();face.points.forEach((p,i)=>i?ctx.lineTo(...p):ctx.moveTo(...p));ctx.closePath();ctx.fillStyle=face.color;ctx.fill();ctx.strokeStyle=face.color;ctx.lineWidth=.45;ctx.stroke();}
  }
  function tick(now){if(!running||disposed)return;if(!previous)previous=now;if(now-previous>=1000/30){time+=(now-previous)/1000;previous=now;draw();}frame=requestAnimationFrame(tick);}
  const observer=new ResizeObserver(draw);observer.observe(canvas);draw();
  return {
    setMode(mode){cancelAnimationFrame(frame);running=false;previous=0;if(mode==='reduced')time=0;draw();if(mode==='play'){running=true;frame=requestAnimationFrame(tick);}},
    destroy(){disposed=true;running=false;cancelAnimationFrame(frame);observer.disconnect();}
  };
}

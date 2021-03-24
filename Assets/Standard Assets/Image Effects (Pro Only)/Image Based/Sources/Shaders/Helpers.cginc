
float focusDistance;
float dofAmount;

half ComputeCOC( float d ) 
{	
	//return saturate (dofAmount * max (0.0, abs(d - focusDistance) - fullFocusRange / (noFocusRange - fullFocusRange)));

	
	half dist = d - focusDistance;
	dist *= dist;
	return clamp (dofAmount*saturate(dist), 0.0,1.0);
}
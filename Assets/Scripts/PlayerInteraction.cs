using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public int damage = 1;
    int currentBreakage;
    Transform prevBreak;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(mouseray.origin, mouseray.direction);
            Transform clickObj = raycastHit2D ? raycastHit2D.collider.transform : null;
            if (clickObj && clickObj.tag == "Resource")
            {
                if (prevBreak == null || clickObj != prevBreak)
                {
                    if (clickObj.name == "Tree")
                    {
                        currentBreakage = 9;
                    }
                }
                if (clickObj == prevBreak)
                {
                    currentBreakage -= damage;
                    if (currentBreakage == 0)
                    {
                        if (clickObj.name == "Tree")
                        {
                            // drop logs n stuff
                        }
                        Destroy(clickObj.gameObject);
                    }
                }
                prevBreak = clickObj;
            }
        }
    }
}

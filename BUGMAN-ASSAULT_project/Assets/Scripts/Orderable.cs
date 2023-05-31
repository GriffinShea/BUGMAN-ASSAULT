using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orderable : MonoBehaviour
{
    private string orderState;
    private string orderableMainScript;
    private string selectionName;
    private string orderString;

    public System.Func<bool> ClearOrder;

    public void SetOrderState(string os) { orderState = os; }
    public string GetOrderState() { return orderState; }

    public void SetUp(string s, string o, System.Func<bool> C) {
        orderState = "";
        selectionName = s;
        orderString = o;
        ClearOrder = C;
        return;
    }

    public bool DoOrder()
    {
        bool completionStatus = false;
        if (orderState == "move") {
            completionStatus = GetComponent<MoveOrder>().MoveUpdate();
		}

        if (completionStatus) {
            orderState = "";
		}

        return orderState == "";
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonUp(0)) {
            General.Select(gameObject, selectionName, orderString);
        }
        return;
    }
}

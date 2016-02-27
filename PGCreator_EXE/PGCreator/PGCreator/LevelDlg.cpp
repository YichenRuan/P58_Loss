// LevelDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "LevelDlg.h"
#include "afxdialogex.h"


// CLevelDlg 对话框

IMPLEMENT_DYNAMIC(CLevelDlg, CDialog)

CLevelDlg::CLevelDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CLevelDlg::IDD, pParent)
{
	ItemChangeOn = FALSE;
	num_checked = 0;
}

CLevelDlg::~CLevelDlg()
{
}

void CLevelDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CLevelDlg, CDialog)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_LEVEL, &CLevelDlg::OnItemchangedListLevel)
END_MESSAGE_MAP()


// CLevelDlg 消息处理程序


BOOL CLevelDlg::OnInitDialog()
{
	CListCtrl* p_Llevel = (CListCtrl*)GetDlgItem(IDC_LIST_LEVEL);
	DWORD dwStyle = p_Llevel->GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT;
	dwStyle |= LVS_EX_GRIDLINES;
	dwStyle |= LVS_EX_CHECKBOXES;
	p_Llevel->SetExtendedStyle(dwStyle); 
	p_Llevel->InsertColumn(0,L"empty", LVCFMT_CENTER,90);
	p_Llevel->InsertColumn(1,L" ", LVCFMT_CENTER,22);
	p_Llevel->InsertColumn(2,L"标高 (m)", LVCFMT_CENTER,187);
	p_Llevel->InsertColumn(3,L"上方楼层", LVCFMT_CENTER,188);
	p_Llevel->DeleteColumn(0);
	return TRUE;
}

void CLevelDlg::InFileInterpret(char* inFile,std::queue<int> &levelQueue)
{
	USES_CONVERSION;
	CListCtrl* p_Llevel = (CListCtrl*)GetDlgItem(IDC_LIST_LEVEL);
	int num_level = levelQueue.size()-1;
	floors = new int[num_level];
	CString temp;
	for (int i = 0;i<num_level;++i)
	{
		p_Llevel->InsertItem(i,L" ");
		p_Llevel->SetItemText(i,1,A2T(inFile + levelQueue.front()));
		levelQueue.pop();
		temp.Format(_T("%d"),i + 1);
		floors[i] = i + 1;
		p_Llevel->SetItemText(i,2,temp + L"F");
		p_Llevel->SetCheck(i);
		++num_checked;
	}
	currRoof = num_level - 1;
	p_Llevel->SetItemText(currRoof,2,L"屋顶");
	ItemChangeOn = TRUE;
}


void CLevelDlg::OnItemchangedListLevel(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	// TODO: 在此添加控件通知处理程序代码
	*pResult = 0;
	CListCtrl* p_Llevel = (CListCtrl*)GetDlgItem(IDC_LIST_LEVEL);
	int nRow = pNMLV->iItem;
	int nItem = p_Llevel->GetItemCount();
	if(ItemChangeOn)										//return if called during initialization
	{
		CString temp;
		if((pNMLV->uOldState & INDEXTOSTATEIMAGEMASK(1))		/* old state : unchecked */ 
			&& (pNMLV->uNewState & INDEXTOSTATEIMAGEMASK(2)))	/* new state : checked */ 
		{ 
			++num_checked;
			if (num_checked == 2)	return;
			if (currRoof < nRow)
			{
				temp.Format(_T("%d"),floors[currRoof]);
				p_Llevel->SetItemText(currRoof,2,temp + L"F");
				p_Llevel->SetItemText(nRow,2,L"屋顶");
				floors[nRow] = -floors[nRow];
				++floors[nRow];
				for (int i = nRow + 1 ; i < nItem ; ++i)
				{
					--floors[i];
				}
				currRoof = nRow;
			}
			else
			{
				floors[nRow] = -floors[nRow];
				temp.Format(_T("%d"),++floors[nRow]);
				p_Llevel->SetItemText(nRow,2,temp + L"F");
				for (int i = nRow + 1 ; i < nItem; ++i)
				{
					if (0 < floors[i])
					{
						temp.Format(_T("%d"),++floors[i]);
						p_Llevel->SetItemText(i,2,temp + L"F");
					}
					else	--floors[i];
				}
				p_Llevel->SetItemText(currRoof,2,L"屋顶");
			}
		} 
		else if((pNMLV->uOldState & INDEXTOSTATEIMAGEMASK(2))	/* old state : checked */ 
			&& (pNMLV->uNewState & INDEXTOSTATEIMAGEMASK(1)))	/* new state : unchecked */ 
		{ 
			--num_checked;
			if (num_checked == 1)
			{
				AfxMessageBox(L"楼层数最小为1");
				p_Llevel->SetCheck(nRow);
				return;
			}
			p_Llevel->SetItemText(nRow,2,L"- -");
			--floors[nRow];										//assert:before decrement, 0 < floors[nRow]
			floors[nRow] = -floors[nRow];						//assert:all checked levels have positive floor values
			for (int i = nRow + 1; i < nItem; ++i)
			{
				if (0 < floors[i])								//assert:1 < floors[i]
				{
					temp.Format(_T("%d"),--floors[i]);
					p_Llevel->SetItemText(i,2,temp + L"F");
				}
				else if (floors[i] < 0)
				{
					++floors[i];
				}
			}
			if (nRow == currRoof)
			{
				for (int j = nRow; 0 < j; --j)
				{
					if (0 < floors[j])
					{
						p_Llevel->SetItemText(j,2,L"屋顶");
						currRoof = j;
						break;
					}
				}
			}
			else p_Llevel->SetItemText(currRoof,2,L"屋顶");
		}
	}
}

void CLevelDlg::OutputInfo(FILE* fp)
{
	CListCtrl* p_Llevel = (CListCtrl*)GetDlgItem(IDC_LIST_LEVEL);
	int nItem = p_Llevel->GetItemCount();
	for (int i=0;i<nItem;++i)
	{
		if (floors[i] <= 0)
		{
			fprintf_s(fp,"%d\t",i);
		}
	}
	fprintf_s(fp,"\n");
}

BOOL CLevelDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}

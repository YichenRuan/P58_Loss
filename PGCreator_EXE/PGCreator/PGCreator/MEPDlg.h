#pragma once
#include "AddMEPDlg.h"
#include <list>
using namespace std;

struct MEPNode
{
	int MEPCode;
	int Cate;
	int Type;
};

// CMEPDlg 对话框

class CMEPDlg : public CDialog
{
	DECLARE_DYNAMIC(CMEPDlg)

public:
	CMEPDlg(CWnd* pParent = NULL);   // 标准构造函数
	virtual ~CMEPDlg();

// 对话框数据
	enum { IDD = IDD_MEP_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnInitDialog();
	afx_msg void OnClickedButtonAdd();

	CAddMEPDlg m_addMEPDlg;
	list<CString> MEPComp;
	list<CString> cate;
	list<list<CString>> type;
public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	void InFileInterpret(char* inFile, int startPoint);
	void AddItem();
	int num_entry;
	list<MEPNode> nodes;
	afx_msg void OnClickedButtonDelete();
	void OutputInfo(FILE* fp_b);
};

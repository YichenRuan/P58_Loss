#pragma once
#include <list>

// CFireProDlg �Ի���

struct FPCombo
{
	int index;
	CString name;
	FPCombo(int index,CString name)
	{
		this->index = index;
		this->name = name;
	}
};


class CFireProDlg : public CDialog
{
	DECLARE_DYNAMIC(CFireProDlg)

public:
	CFireProDlg(CWnd* pParent = NULL);   // ��׼���캯��
	virtual ~CFireProDlg();
	void In2FileInterpret(char* in2File);

// �Ի�������
	enum { IDD = IDD_FIREPRO_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

	DECLARE_MESSAGE_MAP()

private:
	std::list<FPCombo>* up;
	std::list<FPCombo>* down;
	int currCate, currUp, currDown;
	int num_cate;
public:
	virtual BOOL OnInitDialog();
	afx_msg void OnClickListCate(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnClickListUp(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnKillfocusListUp(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnClickListDown(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnKillfocusListDown(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnClickedButtonGodown();
	afx_msg void OnClickedButtonGoup();
	afx_msg void OnClickedButtonAddall();
	afx_msg void OnClickedButtonDeleall();
	void OutputInfo(FILE* fp);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
};
